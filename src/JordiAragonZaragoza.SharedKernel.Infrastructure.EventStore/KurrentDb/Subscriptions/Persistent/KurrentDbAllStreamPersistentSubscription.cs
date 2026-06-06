namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using Grpc.Core;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;
    using KurrentDB.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using ExecutionContext = JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces.ExecutionContext;

    /// <summary>
    /// Persistent subscription to the $all stream in KurrentDB using a consumer group pattern.
    /// Implements horizontal scalability where multiple instances can subscribe to the same group name.
    ///
    /// Design: This implementation uses KurrentDB's native persistent subscription feature which:
    /// - Maintains state on the server (no client-side checkpoints needed)
    /// - Supports consumer groups for automatic event distribution
    /// - Enables horizontal scaling by distributing events among worker instances
    /// - Handles acknowledgment tracking automatically.
    ///
    /// Failure handling: On event processing failure, a NACK with action Retry is sent.
    /// The server retries delivery up to <see cref="KurrentDbAllStreamPersistentSubscriptionOptions.MaxRetryCount"/> times,
    /// after which it automatically parks the event to the dead-letter stream
    /// ($persistentsubscription-{groupName}-parked). No client-side retry counting is needed.
    /// </summary>
    public class KurrentDbAllStreamPersistentSubscription
    {
        private readonly KurrentDBPersistentSubscriptionsClient persistentSubscriptionsClient;
        private readonly IServiceIdentityProvider serviceIdentityProvider;
        private readonly ILogger<KurrentDbAllStreamPersistentSubscription> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private KurrentDbAllStreamPersistentSubscriptionOptions subscriptionOptions = default!;

        // isRunning: true from first successful subscribe until graceful stop (stoppingToken cancelled).
        // isDropped: true when the server drops an active subscription; reset to false on reconnect.
        private bool isRunning;
        private bool isDropped;

        // Captured in SubscribeToAllAsync so the drop handler can check for graceful shutdown
        // without needing a parameter (the callback signature does not carry a CancellationToken).
        private CancellationToken stoppingToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="KurrentDbAllStreamPersistentSubscription"/> class.
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory for creating scoped DI containers.</param>
        /// <param name="persistentSubscriptionsClient">The KurrentDB persistent subscriptions client.</param>
        /// <param name="serviceIdentityProvider">The service identity provider.</param>
        /// <param name="logger">The logger instance.</param>
        public KurrentDbAllStreamPersistentSubscription(
            IServiceScopeFactory serviceScopeFactory,
            KurrentDBPersistentSubscriptionsClient persistentSubscriptionsClient,
            IServiceIdentityProvider serviceIdentityProvider,
            ILogger<KurrentDbAllStreamPersistentSubscription> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.persistentSubscriptionsClient = persistentSubscriptionsClient ?? throw new ArgumentNullException(nameof(persistentSubscriptionsClient));
            this.serviceIdentityProvider = serviceIdentityProvider ?? throw new ArgumentNullException(nameof(serviceIdentityProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GroupName => this.subscriptionOptions.GroupName;

        /// <summary>
        /// Connects to the $all stream persistent subscription group and registers the event callback.
        /// Returns as soon as the subscription is established; the caller is responsible for
        /// keeping the process alive (see <see cref="AllStreamPersistentSubscriptionBackgroundWorker"/>).
        /// </summary>
        /// <param name="subscriptionOptions">The subscription options including group name and filters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes when the subscription is established.</returns>
        public async Task SubscribeToAllAsync(
            KurrentDbAllStreamPersistentSubscriptionOptions subscriptionOptions,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(subscriptionOptions, nameof(subscriptionOptions));

            if (this.isRunning)
            {
                this.logger.LogWarning("Already subscribed to group '{GroupName}'", subscriptionOptions.GroupName);
                return;
            }

            this.subscriptionOptions = subscriptionOptions;
            this.stoppingToken = cancellationToken;
            cancellationToken.Register(() => this.isRunning = false);

            this.logger.LogInformation("Subscribing to consumer group '{GroupName}'", this.GroupName);

            await this.ConnectToGroupAsync(cancellationToken);

            this.isRunning = true;

            this.logger.LogInformation("Persistent subscription group '{GroupName}' started", this.GroupName);
        }

        /// <summary>
        /// Detects whether an exception from <see cref="CreateSubscriptionGroupAsync"/> indicates
        /// the group was already created by a concurrent instance (race condition on startup).
        /// KurrentDB surfaces this as an <see cref="InvalidOperationException"/> wrapping an
        /// <see cref="RpcException"/> with <see cref="StatusCode.AlreadyExists"/>.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns>True if the group already exists; otherwise false.</returns>
        private static bool IsAlreadyExistsException(Exception exception)
        {
            var rpcException = (exception as InvalidOperationException)?.InnerException as RpcException
                            ?? exception as RpcException;

            return rpcException?.StatusCode == StatusCode.AlreadyExists;
        }

        /// <summary>
        /// Enriches the current Activity with correlation and tracing information from event metadata.
        /// </summary>
        /// <param name="activity">The Activity to enrich with tracing information.</param>
        /// <param name="metadata">The event metadata containing tracing context.</param>
        private static void EnrichActivity(Activity? activity, EventStoreMetadata? metadata)
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
        /// Checks if an event is a system event (event type starts with $).
        /// System events should not be processed as domain events.
        /// </summary>
        /// <param name="resolvedEvent">The resolved event to check.</param>
        /// <returns>True if the event is a system event; otherwise false.</returns>
        private static bool IsSystemEvent(ResolvedEvent resolvedEvent)
        {
            return resolvedEvent.Event.EventType.StartsWith('$');
        }

        /// <summary>
        /// Establishes the initial connection to the persistent subscription group.
        /// Creates the group on KurrentDB if it does not exist yet.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the connection operation.</returns>
        private async Task ConnectToGroupAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Connecting to persistent subscription group '{GroupName}'",
                    this.GroupName);

                await this.SubscribeToGroupAsync(cancellationToken);

                this.logger.LogInformation(
                    "Connected to persistent subscription group '{GroupName}' on $all stream",
                    this.GroupName);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error connecting to persistent subscription group '{GroupName}': {ExceptionMessage}",
                    this.GroupName,
                    ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Attempts to subscribe to the existing consumer group.
        /// If the group does not exist, creates it first and then retries the subscription.
        /// If creation races with another instance (group already exists), the exception is
        /// swallowed and the subscription is attempted directly.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the subscribe-or-create-then-subscribe operation.</returns>
        private async Task SubscribeToGroupAsync(CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.persistentSubscriptionsClient.SubscribeToAllAsync(
                    this.subscriptionOptions.GroupName,
                    this.HandleEventCallbackAsync,
                    this.HandleSubscriptionDropped,
                    this.subscriptionOptions.Credentials,
                    this.subscriptionOptions.BufferSize,
                    cancellationToken);
            }
            catch (PersistentSubscriptionNotFoundException)
            {
#pragma warning disable S6667
                this.logger.LogInformation(
                    "Persistent subscription group '{GroupName}' not found. Creating it now.",
                    this.GroupName);
#pragma warning restore S6667

                try
                {
                    await this.CreateSubscriptionGroupAsync(cancellationToken);
                }
                catch (Exception createException) when (!IsAlreadyExistsException(createException))
                {
                    this.logger.LogError(
                        createException,
                        "Failed to create persistent subscription group '{GroupName}'",
                        this.GroupName);
                    throw;
                }

                // If we reach here the group was either just created by us, or already existed
                // due to a concurrent instance — either way we can subscribe now.
                this.logger.LogInformation(
                    "Persistent subscription group '{GroupName}' ready (created or already existed).",
                    this.GroupName);

                _ = await this.persistentSubscriptionsClient.SubscribeToAllAsync(
                    this.subscriptionOptions.GroupName,
                    this.HandleEventCallbackAsync,
                    this.HandleSubscriptionDropped,
                    this.subscriptionOptions.Credentials,
                    this.subscriptionOptions.BufferSize,
                    cancellationToken);

                this.logger.LogInformation(
                    "Successfully subscribed to persistent subscription group '{GroupName}'",
                    this.GroupName);
            }
        }

        /// <summary>
        /// Creates the persistent subscription group on KurrentDB for the $all stream.
        /// Uses the filter options from <see cref="subscriptionOptions"/> if provided.
        /// The <see cref="KurrentDbAllStreamPersistentSubscriptionOptions.MaxRetryCount"/> is passed
        /// to the server so that KurrentDB automatically parks events after the configured number of
        /// failed delivery attempts, without requiring any client-side retry counting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the creation operation.</returns>
        private Task CreateSubscriptionGroupAsync(CancellationToken cancellationToken)
        {
            // MaxRetryCount tells the server how many times to retry a NACKed event before
            // automatically moving it to the dead-letter (parked) stream.
            // This is the only mechanism needed to prevent infinite retry loops.
            var settings = new PersistentSubscriptionSettings(
                resolveLinkTos: this.subscriptionOptions.ResolveLinkTos,
                maxRetryCount: this.subscriptionOptions.MaxRetryCount);

            return this.subscriptionOptions.FilterOptions is not null
                ? this.persistentSubscriptionsClient.CreateToAllAsync(
                    this.subscriptionOptions.GroupName,
                    this.subscriptionOptions.FilterOptions.Filter,
                    settings,
                    userCredentials: this.subscriptionOptions.Credentials,
                    cancellationToken: cancellationToken)
                : this.persistentSubscriptionsClient.CreateToAllAsync(
                    this.subscriptionOptions.GroupName,
                    settings,
                    userCredentials: this.subscriptionOptions.Credentials,
                    cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Callback invoked when an event is available in the persistent subscription.
        /// On success, ACK is sent to advance the server-side position.
        /// On failure, NACK with action Retry is sent. The server retries up to
        /// <see cref="KurrentDbAllStreamPersistentSubscriptionOptions.MaxRetryCount"/> times,
        /// then parks the event automatically. No client-side retry counting is required.
        ///
        /// Deserialization failures:
        /// - If IgnoreDeserializationErrors is true, the event is ACK'd and skipped with a warning.
        ///   Use this for schema evolution scenarios where unknown or malformed events are expected.
        /// - If IgnoreDeserializationErrors is false, the error propagates to NACK + retry.
        ///   A deserialization failure in a single-context KurrentDB setup is always anomalous
        ///   and should be surfaced, not silently swallowed.
        /// </summary>
        private async Task HandleEventCallbackAsync(
            PersistentSubscription subscription,
            ResolvedEvent resolvedEvent,
            int? retryCount,
            CancellationToken cancellationToken)
        {
            try
            {
                if (IsSystemEvent(resolvedEvent))
                {
                    this.logger.LogDebug(
                        "Skipping system event '{EventType}' at position {Position}",
                        resolvedEvent.Event.EventType,
                        resolvedEvent.Event.Position.CommitPosition);

                    await subscription.Ack(resolvedEvent);
                    return;
                }

                if (this.IsEventWithEmptyData(resolvedEvent))
                {
                    await subscription.Ack(resolvedEvent);
                    return;
                }

                IDomainEvent domainEvent;
                EventStoreMetadata? metadata;

                try
                {
                    (domainEvent, metadata) = SerializerHelper.Deserialize(resolvedEvent);
                }
                catch (Exception deserializeException)
                {
                    if (this.subscriptionOptions.IgnoreDeserializationErrors)
                    {
                        this.logger.LogWarning(
                            deserializeException,
                            "Deserialization error for event '{EventType}' at position {Position} in group '{GroupName}'. " + "Skipping event (IgnoreDeserializationErrors = true).",
                            resolvedEvent.Event.EventType,
                            resolvedEvent.Event.Position.CommitPosition,
                            this.GroupName);

                        await subscription.Ack(resolvedEvent);
                        return;
                    }

                    // IgnoreDeserializationErrors = false: a deserialization failure in a single-context
                    // setup is always a real bug. Propagate to the outer catch → NACK + retry.
                    throw;
                }

                using var activity = EventStoreActivityRestorer.RestoreFrom(metadata, $"Handle {domainEvent.GetType().Name}");

                EnrichActivity(activity, metadata);

                using var scope = this.serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var executionContextService = scope.ServiceProvider.GetRequiredService<IExecutionContextService>();

                var executionContext = metadata?.ToExecutionContext();
                if (executionContext is null)
                {
                    this.logger.LogDebug(
                        "Event {EventType} at position {Position} has no application metadata. Using fallback system context.",
                        resolvedEvent.Event.EventType,
                        resolvedEvent.Event.Position.CommitPosition);

                    executionContext = this.BuildFallbackSystemContext();
                }

                executionContextService.OverrideExecutionContext(executionContext);
                try
                {
                    await unitOfWork.ExecuteInTransactionAsync(
                        async () =>
                        {
                            await eventBus.PublishAsync(domainEvent, cancellationToken);
                            return Result.Success();
                        },
                        cancellationToken);

                    await subscription.Ack(resolvedEvent);

                    this.logger.LogDebug(
                        "Event {EventType} at position {Position} processed and acknowledged",
                        resolvedEvent.Event.EventType,
                        resolvedEvent.Event.Position.CommitPosition);
                }
                finally
                {
                    executionContextService.ClearExecutionContext();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The host is shutting down. Do not NACK — the server will redeliver
                // to another consumer. Do not log as an error.
        #pragma warning disable S6667
                this.logger.LogInformation(
                    "Event {EventType} at position {Position} processing cancelled due to host shutdown",
                    resolvedEvent.Event.EventType,
                    resolvedEvent.Event.Position.CommitPosition);
        #pragma warning restore S6667
            }
        #pragma warning disable CA1031
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error processing event {EventType} from group '{GroupName}' (retry count: {RetryCount}): {ExceptionMessage}",
                    resolvedEvent.Event.EventType,
                    this.GroupName,
                    retryCount,
                    ex.Message);

                // NACK with Retry. The server tracks the retry count and parks the event
                // automatically after MaxRetryCount attempts, preventing infinite loops.
                try
                {
                    await subscription.Nack(PersistentSubscriptionNakEventAction.Retry, ex.Message, resolvedEvent);
                }
                catch (Exception nackEx)
                {
                    this.logger.LogError(
                        nackEx,
                        "Failed to NACK event {EventType} in group '{GroupName}'",
                        resolvedEvent.Event.EventType,
                        this.GroupName);
                }
            }
        #pragma warning restore CA1031
        }

        /// <summary>
        /// Callback invoked when the persistent subscription is dropped by the server or due to an error.
        /// Marks the subscription as dropped and starts a background reconnect loop with exponential backoff.
        /// If the drop reason is <see cref="SubscriptionDroppedReason.Disposed"/> (i.e. graceful host shutdown),
        /// no reconnect is attempted.
        /// </summary>
        /// <param name="subscription">The dropped persistent subscription.</param>
        /// <param name="reason">The reason for the drop.</param>
        /// <param name="exception">The exception that caused the drop, if any.</param>
        private void HandleSubscriptionDropped(
            PersistentSubscription subscription,
            SubscriptionDroppedReason reason,
            Exception? exception)
        {
            this.logger.LogWarning(
                exception,
                "Persistent subscription group '{GroupName}' dropped with reason '{DropReason}'",
                this.GroupName,
                reason);

            if (reason == SubscriptionDroppedReason.Disposed || this.stoppingToken.IsCancellationRequested)
            {
                return;
            }

            this.isDropped = true;

            _ = Task.Run(
                async () =>
                {
                    var initialDelay = reason == SubscriptionDroppedReason.ServerError
                        ? TimeSpan.FromSeconds(2)
                        : TimeSpan.FromSeconds(10);

                    await this.ReconnectWithBackoffAsync(initialDelay, this.stoppingToken);
                },
                this.stoppingToken);
        }

        /// <summary>
        /// Reconnects to the persistent subscription group with exponential backoff.
        /// Retries until the subscription is restored or the stopping token is cancelled.
        /// </summary>
        /// <param name="initialDelay">The delay to wait before the first reconnect attempt.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the reconnect loop.</returns>
        private async Task ReconnectWithBackoffAsync(TimeSpan initialDelay, CancellationToken cancellationToken)
        {
            var delay = initialDelay;

            while (this.isRunning && this.isDropped && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.logger.LogInformation(
                        "Reconnecting to persistent subscription group '{GroupName}' in {Delay}s...",
                        this.GroupName,
                        delay.TotalSeconds);

                    await Task.Delay(delay, cancellationToken);

                    await this.SubscribeToGroupAsync(cancellationToken);

                    this.isDropped = false;

                    this.logger.LogInformation(
                        "Successfully reconnected to persistent subscription group '{GroupName}'",
                        this.GroupName);

                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
#pragma warning disable CA1031
                catch (Exception ex)
                {
                    this.logger.LogError(
                        ex,
                        "Reconnect attempt for group '{GroupName}' failed: {ExceptionMessage}. Retrying in {Delay}s...",
                        this.GroupName,
                        ex.Message,
                        delay.TotalSeconds);

                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                }
#pragma warning restore CA1031
            }
        }

        /// <summary>
        /// Checks if an event has empty data (system event or tombstone).
        /// </summary>
        /// <param name="resolvedEvent">The resolved event to check.</param>
        /// <returns>True if the event has empty data; otherwise false.</returns>
        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length != 0)
            {
                return false;
            }

            this.logger.LogInformation(
                "Event without data received in group '{GroupName}' at position {Position}",
                this.GroupName,
                resolvedEvent.Event.Position.CommitPosition);

            return true;
        }

        /// <summary>
        /// Builds a fallback system execution context for events that lack metadata.
        /// </summary>
        /// <returns>An ExecutionContext with system defaults.</returns>
        private ExecutionContext BuildFallbackSystemContext()
        {
            this.logger.LogInformation(
                "Building fallback system execution context for event without metadata in group '{GroupName}'",
                this.GroupName);

            var systemTenantId = SystemConstants.SystemTenantId;

            return new ExecutionContext(
                actorId: ExecutionContext.CreateServiceActorId("event-store-persistent-subscription"),
                actorType: ActorType.System,
                executor: this.serviceIdentityProvider.GetName(),
                executorType: ExecutorType.Worker,
                correlationId: Guid.NewGuid(),
                causationId: null,
                scopeContext: new ScopeContext(systemTenantId, null, null));
        }
    }
}