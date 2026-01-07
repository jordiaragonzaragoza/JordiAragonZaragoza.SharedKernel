namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint;
    using KurrentDB.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class KurrentDbAllStreamSubscription
    {
        private readonly KurrentDBClient kurrentDbClient;
        private readonly ILogger<KurrentDbAllStreamSubscription> logger;
        private readonly IDateTime datetime;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private KurrentDbAllStreamSubscriptionOptions subscriptionOptions = default!;
        private bool isSubscribed;

        public KurrentDbAllStreamSubscription(
            IServiceScopeFactory serviceScopeFactory,
            KurrentDBClient eventStoreClient,
            ILogger<KurrentDbAllStreamSubscription> logger,
            IDateTime datetime)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.kurrentDbClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.datetime = datetime ?? throw new ArgumentNullException(nameof(datetime));
        }

        private Guid SubscriptionId => this.subscriptionOptions.SubscriptionId;

        public async Task SubscribeToAllAsync(KurrentDbAllStreamSubscriptionOptions subscriptionOptions, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(subscriptionOptions, nameof(subscriptionOptions));

            if (this.isSubscribed)
            {
                this.logger.LogWarning("Already subscribed to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);
                return;
            }

            // see: https://github.com/dotnet/runtime/issues/36063
            await Task.Yield();
            this.subscriptionOptions = subscriptionOptions;

            this.logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

            // Required to get scoped services on a background service.
            using var scope = this.serviceScopeFactory.CreateScope();
            var checkpointRepository = scope.ServiceProvider.GetRequiredService<IRepository<Checkpoint, Guid>>();

            var checkpoint = await checkpointRepository.GetByIdAsync(this.SubscriptionId, cancellationToken).ConfigureAwait(false);

            // Use the iterable subscription pattern that maintains the connection continuously
            // This processes historical events (catch-up) and then listens for new events (live)
            await using var subscription = this.kurrentDbClient.SubscribeToAll(
                checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Position, checkpoint.Position)),
                subscriptionOptions.ResolveLinkTos,
                subscriptionOptions.FilterOptions,
                subscriptionOptions.Credentials,
                cancellationToken);

            this.isSubscribed = true;

            this.logger.LogInformation("Subscription to all '{SubscriptionId}' started - processing catch-up and listening for live events", this.SubscriptionId);

            // Process messages continuously: first the catch-up (historical), then live events
            await foreach (var message in subscription.Messages.WithCancellation(cancellationToken))
            {
                await this.HandleMessageAsync(message, checkpointRepository, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandleMessageAsync(StreamMessage message, IRepository<Checkpoint, Guid> checkpointRepository, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case StreamMessage.Event(var resolvedEvent):
                    await this.HandleEventAsync(resolvedEvent, checkpointRepository, cancellationToken).ConfigureAwait(false);
                    break;

                case StreamMessage.CaughtUp:
                    this.logger.LogInformation("Subscription '{SubscriptionId}' caught up - transitioning to live mode", this.SubscriptionId);
                    break;

                case StreamMessage.FellBehind:
                    this.logger.LogWarning("Subscription '{SubscriptionId}' fell behind - processing may be slower than incoming events", this.SubscriptionId);
                    break;

                default:
                    this.logger.LogDebug("Received message type '{MessageType}' for subscription '{SubscriptionId}'", message.GetType().Name, this.SubscriptionId);
                    break;
            }
        }

        private async Task HandleEventAsync(ResolvedEvent resolvedEvent, IRepository<Checkpoint, Guid> checkpointRepository, CancellationToken cancellationToken)
        {
            try
            {
                if (this.IsEventWithEmptyData(resolvedEvent))
                {
                    return;
                }

                var domainEvent = SerializerHelper.Deserialize(resolvedEvent);

                // Required to get scoped services on a background service.
                using var scope = this.serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                // This transaction is required to commit changes to multiple repositories atomically.
                await unitOfWork.ExecuteInTransactionAsync(
                    async () =>
                    {
                        // publish event to internal event bus
                        await eventBus.PublishAsync(domainEvent, cancellationToken);

                        var existingCheckpoint = await checkpointRepository.GetByIdAsync(this.SubscriptionId, cancellationToken);
                        if (existingCheckpoint is not null)
                        {
                            existingCheckpoint.Position = resolvedEvent.Event.Position.CommitPosition;
                            existingCheckpoint.CheckpointedAtOnUtc = this.datetime.UtcNow;

                            await checkpointRepository.UpdateAsync(existingCheckpoint, cancellationToken)
                            .ConfigureAwait(false);

                            this.logger.LogInformation("Updated checkpoint: {Checkpoint}", existingCheckpoint);

                            return Result.Success();
                        }

                        var checkpoint = new Checkpoint(this.SubscriptionId, resolvedEvent.Event.Position.CommitPosition, this.datetime.UtcNow);
                        _ = await checkpointRepository.AddAsync(checkpoint, cancellationToken)
                            .ConfigureAwait(false);

                        this.logger.LogInformation("Added checkpoint: {Checkpoint}", checkpoint);

                        return Result.Success();
                        },
                    cancellationToken);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", exception.Message, exception.StackTrace);

                // if you're fine with dropping some events instead of stopping subscription
                // then you can add some logic if error should be ignored
                throw;
            }
        }

        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length != 0)
            {
                return false;
            }

            this.logger.LogInformation("Event without data received");

            return true;
        }
    }
}