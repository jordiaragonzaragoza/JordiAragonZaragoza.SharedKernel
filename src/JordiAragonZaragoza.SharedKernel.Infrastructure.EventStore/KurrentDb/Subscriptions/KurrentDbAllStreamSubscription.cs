namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Helpers;
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
        private readonly object resubscribeLock = new();
        private readonly IServiceScopeFactory serviceScopeFactory;
        private KurrentDbAllStreamSubscriptionOptions subscriptionOptions = default!;
        private CancellationToken cancellationToken;

        public KurrentDbAllStreamSubscription(
            IServiceScopeFactory serviceScopeFactory,
            KurrentDBClient eventStoreClient,
            EventTypeMapper eventTypeMapper,
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

            // see: https://github.com/dotnet/runtime/issues/36063
            await Task.Yield();
            this.subscriptionOptions = subscriptionOptions;
            this.cancellationToken = cancellationToken;

            this.logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

            // Required to get scoped services on a background service.
            using var scope = this.serviceScopeFactory.CreateScope();
            var checkpointRepository = scope.ServiceProvider.GetRequiredService<IRepository<Checkpoint, Guid>>();

            var checkpoint = await checkpointRepository.GetByIdAsync(this.SubscriptionId, cancellationToken).ConfigureAwait(false);

            _ = await this.kurrentDbClient.SubscribeToAllAsync(
                checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Position, checkpoint.Position)),
                this.HandleEventAsync,
                subscriptionOptions.ResolveLinkTos,
                this.HandleDrop,
                subscriptionOptions.FilterOptions,
                subscriptionOptions.Credentials,
                cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Subscription to all '{SubscriptionId}' started", this.SubscriptionId);
        }

        private async Task HandleEventAsync(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
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

                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var checkpointRepository = scope.ServiceProvider.GetRequiredService<IRepository<Checkpoint, Guid>>();

                // TODO: Add Unit of Work pattern to commit changes to multiple repositories atomically.
                // publish event to internal event bus
                await eventBus.PublishAsync(domainEvent, cancellationToken);

                var existingCheckpoint = await checkpointRepository.GetByIdAsync(this.SubscriptionId, cancellationToken);
                if (existingCheckpoint is not null)
                {
                    existingCheckpoint.Position = resolvedEvent.Event.Position.CommitPosition;
                    existingCheckpoint.CheckpointedAtOnUtc = this.datetime.UtcNow;

                    await checkpointRepository.UpdateAsync(existingCheckpoint, cancellationToken)
                    .ConfigureAwait(false);

                    this.logger.LogInformation("Added checkpoint: {Checkpoint}", existingCheckpoint);

                    return;
                }

                var checkpoint = new Checkpoint(this.SubscriptionId, resolvedEvent.Event.Position.CommitPosition, this.datetime.UtcNow);
                _ = await checkpointRepository.AddAsync(checkpoint, cancellationToken)
                    .ConfigureAwait(false);

                this.logger.LogInformation("Added checkpoint: {Checkpoint}", checkpoint);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", exception.Message, exception.StackTrace);

                // if you're fine with dropping some events instead of stopping subscription
                // then you can add some logic if error should be ignored
                throw;
            }
        }

        private void HandleDrop(StreamSubscription subscription, SubscriptionDroppedReason reason, Exception? exception)
        {
            if (exception is RpcException { StatusCode: StatusCode.Cancelled })
            {
                this.logger.LogWarning(
                    "Subscription to all '{SubscriptionId}' dropped by client",
                    this.SubscriptionId);

                return;
            }

            this.logger.LogError(
                exception,
                "Subscription to all '{SubscriptionId}' dropped with '{StatusCode}' and '{Reason}'",
                this.SubscriptionId,
                (exception as RpcException)?.StatusCode ?? StatusCode.Unknown,
                reason);

            this.Resubscribe();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ok for BackgroundService and Resubscribe")]
        private void Resubscribe()
        {
            // You may consider adding a max resubscribe count if you want to fail process
            // instead of retrying until database is up
            while (true)
            {
                var resubscribed = false;
                try
                {
                    Monitor.Enter(this.resubscribeLock);

                    // No synchronization context is needed to disable synchronization context.
                    // That enables running asynchronous method not causing deadlocks.
                    // As this is a background process then we don't need to have async context here.
                    using (NoSynchronizationContextScopeHelper.Enter())
                    {
                        this.SubscribeToAllAsync(this.subscriptionOptions, this.cancellationToken).Wait(this.cancellationToken);
                    }

                    resubscribed = true;
                }
                catch (Exception exception)
                {
                    this.logger.LogWarning(
                        exception,
                        "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'",
                        this.SubscriptionId,
                        exception.Message,
                        exception.StackTrace);
                }
                finally
                {
                    Monitor.Exit(this.resubscribeLock);
                }

                if (resubscribed)
                {
                    break;
                }

                // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
                // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
#pragma warning disable CA5394 // Do not use insecure randomness
                Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
#pragma warning restore CA5394 // Do not use insecure randomness
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