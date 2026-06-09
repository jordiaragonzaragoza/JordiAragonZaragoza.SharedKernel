namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint;
    using KurrentDB.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using ExecutionContext = JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces.ExecutionContext;

    public class KurrentDbAllStreamCatchUpSubscription
    {
        private readonly KurrentDBClient kurrentDbClient;
        private readonly IServiceIdentityProvider serviceIdentityProvider;

        private readonly ILogger<KurrentDbAllStreamCatchUpSubscription> logger;
        private readonly IDateTime datetime;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private KurrentDbAllStreamCatchUpSubscriptionOptions subscriptionOptions = default!;
        private int isSubscribed; // 0 = false, 1 = true

        public KurrentDbAllStreamCatchUpSubscription(
            IServiceScopeFactory serviceScopeFactory,
            KurrentDBClient eventStoreClient,
            IServiceIdentityProvider serviceIdentityProvider,
            ILogger<KurrentDbAllStreamCatchUpSubscription> logger,
            IDateTime datetime)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.kurrentDbClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.serviceIdentityProvider = serviceIdentityProvider ?? throw new ArgumentNullException(nameof(serviceIdentityProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.datetime = datetime ?? throw new ArgumentNullException(nameof(datetime));
        }

        private Guid SubscriptionId => this.subscriptionOptions.SubscriptionId;

        public async Task SubscribeToAllAsync(KurrentDbAllStreamCatchUpSubscriptionOptions subscriptionOptions, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(subscriptionOptions, nameof(subscriptionOptions));

            if (Interlocked.CompareExchange(ref this.isSubscribed, 1, 0) == 1)
            {
                this.logger.LogWarning("Already subscribed to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

                return;
            }

            try
            {
                this.subscriptionOptions = subscriptionOptions;

                this.logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

                // Required to get scoped services on a background service.
                using var startupScope = this.serviceScopeFactory.CreateScope();
                var startupCheckpointRepository = startupScope.ServiceProvider.GetRequiredService<IRepository<Checkpoint, Guid>>();

                var checkpoint = await startupCheckpointRepository.GetByIdAsync(this.SubscriptionId, cancellationToken).ConfigureAwait(false);

                // Use the iterable subscription pattern that maintains the connection continuously
                // This processes historical events (catch-up) and then listens for new events (live)
                await using var subscription = this.kurrentDbClient.SubscribeToAll(
                    checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Position, checkpoint.Position)),
                    subscriptionOptions.ResolveLinkTos,
                    subscriptionOptions.FilterOptions,
                    subscriptionOptions.Credentials,
                    cancellationToken);

                this.logger.LogInformation("Subscription to all '{SubscriptionId}' started - processing catch-up and listening for live events", this.SubscriptionId);

                // Process messages continuously: first the catch-up (historical), then live events
                await foreach (var message in subscription.Messages.WithCancellation(cancellationToken))
                {
                    await this.HandleMessageAsync(message, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                Interlocked.Exchange(ref this.isSubscribed, 0); // allows reconection
            }
        }

        private static void EnrichActivity(
            Activity? activity,
            EventStoreMetadata? metadata)
        {
            if (activity is null || metadata is null)
            {
                return;
            }

            activity.SetTag("actor.id", metadata.ActorId);
            activity.SetTag("actor.type", metadata.ActorType);
            activity.SetTag("executor", metadata.Executor);
            activity.SetTag("executor.type", metadata.ExecutorType);

            activity.SetTag("correlation.id", metadata.CorrelationId.ToString());

            if (metadata.CausationId.HasValue)
            {
                activity.SetTag("causation.id", metadata.CausationId.Value.ToString());
            }

            activity.SetTag("tenant.id", metadata.TenantId.ToString());

            if (metadata.PartitionId.HasValue)
            {
                activity.SetTag("partition.id", metadata.PartitionId.Value.ToString());
            }

            if (metadata.DomainId.HasValue)
            {
                activity.SetTag("domain.id", metadata.DomainId.Value.ToString());
            }

            activity.SetTag("event.occurred_at", metadata.DateOccurredOnUtc.ToString("O"));
        }

        /// <summary>
        /// Builds a fallback system execution context for events that lack metadata (e.g., seeds, migrations).
        /// </summary>
        private ExecutionContext BuildFallbackSystemContext()
        {
            this.logger.LogDebug("Building fallback system execution context for event without metadata");

            var systemTenantId = SystemConstants.SystemTenantId;

            return new ExecutionContext(
                actorId: ExecutionContext.CreateServiceActorId("event-store-all-stream-catch-up-subscription"),
                actorType: ActorType.System,
                executor: this.serviceIdentityProvider.GetName(),
                executorType: ExecutorType.Worker,
                correlationId: Guid.CreateVersion7(),
                causationId: null,
                scopeContext: new ScopeContext(systemTenantId, null, null));
        }

        private async Task HandleMessageAsync(StreamMessage message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case StreamMessage.Event(var resolvedEvent):
                    await this.HandleEventAsync(resolvedEvent, cancellationToken).ConfigureAwait(false);
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

        private async Task HandleEventAsync(
            ResolvedEvent resolvedEvent,
            CancellationToken cancellationToken)
        {
            try
            {
                if (this.IsEventWithEmptyData(resolvedEvent))
                {
                    return;
                }

                IDomainEvent domainEvent;
                EventStoreMetadata? metadata;

                try
                {
                    (domainEvent, metadata) = SerializerHelper.Deserialize(resolvedEvent);
                }
                catch (Exception deserializationException)
                {
                    this.logger.LogError(
                        deserializationException,
                        "Deserialization failed for event type '{EventType}' at position {Position}. " + "IgnoreDeserializationErrors={IgnoreDeserializationErrors}",
                        resolvedEvent.Event.EventType,
                        resolvedEvent.Event.Position.CommitPosition,
                        this.subscriptionOptions.IgnoreDeserializationErrors);

                    if (this.subscriptionOptions.IgnoreDeserializationErrors)
                    {
                        // Advance the checkpoint so we don't retry this poison event forever.
                        using var scope = this.serviceScopeFactory.CreateScope();
                        var repository = scope.ServiceProvider
                            .GetRequiredService<IRepository<Checkpoint, Guid>>();

                        await this.UpdateCheckpointAsync(resolvedEvent, repository, cancellationToken);

                        return;
                    }

                    throw; // rethrow to stop subscription if we can't ignore deserialization errors
                }

                using var activity = EventStoreActivityRestorer.RestoreFrom(metadata, $"Handle {domainEvent.GetType().Name}");

                EnrichActivity(activity, metadata);

                // Required to get scoped services on a background service.
                using var eventScope = this.serviceScopeFactory.CreateScope();
                var unitOfWork = eventScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var eventBus = eventScope.ServiceProvider.GetRequiredService<IEventBus>();
                var executionContextService = eventScope.ServiceProvider.GetRequiredService<IExecutionContextService>();
                var checkpointRepository = eventScope.ServiceProvider.GetRequiredService<IRepository<Checkpoint, Guid>>();

                var executionContext = metadata?.ToProcessingExecutionContext(causationId: domainEvent.Id);
                if (executionContext is null)
                {
                    this.logger.LogDebug(
                        "Event {EventType} at position {Position} has no application metadata " +
                        "(KurrentDB-native or legacy event). Using fallback system context.",
                        resolvedEvent.Event.EventType,
                        resolvedEvent.Event.Position.CommitPosition);

                    executionContext = this.BuildFallbackSystemContext();
                }

                executionContextService.OverrideExecutionContext(executionContext);
                try
                {
                    // This transaction is required to commit changes to multiple repositories atomically.
                    await unitOfWork.ExecuteInTransactionAsync(
                        async () =>
                        {
                            // publish event to internal event bus
                            await eventBus.PublishAsync(domainEvent, cancellationToken);

                            await this.UpdateCheckpointAsync(resolvedEvent, checkpointRepository, cancellationToken);

                            return Result.Success();
                        },
                        cancellationToken);
                }
                finally
                {
                    executionContextService.ClearExecutionContext();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Clean shutdown — let it propagate without logging as error
                throw;
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    "Error consuming message: {ExceptionMessage}",
                    exception.Message);

                // if you're fine with dropping some events instead of stopping subscription
                // then you can add some logic if error should be ignored
                throw;
            }
        }

        private async Task UpdateCheckpointAsync(
            ResolvedEvent resolvedEvent,
            IRepository<Checkpoint, Guid> checkpointRepository,
            CancellationToken cancellationToken)
        {
            var existing = await checkpointRepository
                .GetByIdAsync(this.SubscriptionId, cancellationToken);

            if (existing is not null)
            {
                existing.Position = resolvedEvent.Event.Position.CommitPosition;
                existing.CheckpointedAtOnUtc = this.datetime.UtcNow;

                await checkpointRepository.UpdateAsync(existing, cancellationToken);

                this.logger.LogInformation("Updated checkpoint: {Checkpoint}", existing);

                return;
            }

            var checkpoint = new Checkpoint(
                this.SubscriptionId,
                resolvedEvent.Event.Position.CommitPosition,
                this.datetime.UtcNow);

            _ = await checkpointRepository.AddAsync(checkpoint, cancellationToken);

            this.logger.LogInformation("Added checkpoint: {Checkpoint}", checkpoint);
        }

        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length != 0)
            {
                return false;
            }

            this.logger.LogDebug(
                "Event '{EventType}' at position {Position} has no data, skipping",
                resolvedEvent.Event.EventType,
                resolvedEvent.Event.Position.CommitPosition);

            return true;
        }
    }
}