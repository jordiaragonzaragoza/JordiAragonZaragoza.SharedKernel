namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Background worker that manages the lifetime of the persistent subscription to the $all stream.
    ///
    /// Lifecycle:
    /// 1. Calls SubscribeToAllAsync, which connects to KurrentDB and registers the event callback.
    ///    The call returns as soon as the subscription is established; it does not block.
    /// 2. Waits indefinitely on the stoppingToken. The subscription delivers events via its own
    ///    server-side callback mechanism (HandleEventCallbackAsync) while this task is parked.
    /// 3. When the host signals shutdown (stoppingToken cancelled), the Delay throws
    ///    OperationCanceledException, ExecuteAsync returns, and BackgroundService disposes
    ///    the subscription cleanly.
    /// </summary>
    public class AllStreamPersistentSubscriptionBackgroundWorker : BackgroundService
    {
        private readonly KurrentDbAllStreamPersistentSubscription subscription;
        private readonly KurrentDbAllStreamPersistentSubscriptionOptions subscriptionOptions;
        private readonly ILogger<AllStreamPersistentSubscriptionBackgroundWorker> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllStreamPersistentSubscriptionBackgroundWorker"/> class.
        /// </summary>
        /// <param name="subscription">The persistent subscription instance.</param>
        /// <param name="subscriptionOptions">The subscription configuration options.</param>
        /// <param name="logger">The logger instance.</param>
        public AllStreamPersistentSubscriptionBackgroundWorker(
            KurrentDbAllStreamPersistentSubscription subscription,
            KurrentDbAllStreamPersistentSubscriptionOptions subscriptionOptions,
            ILogger<AllStreamPersistentSubscriptionBackgroundWorker> logger)
        {
            this.subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
            this.subscriptionOptions = subscriptionOptions ?? throw new ArgumentNullException(nameof(subscriptionOptions));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation(
                "Starting all stream persistent subscription background worker with group '{GroupName}'",
                this.subscriptionOptions.GroupName);

            await this.subscription.SubscribeToAllAsync(this.subscriptionOptions, stoppingToken);

            // Keep the worker alive until the host signals shutdown.
            // The subscription delivers events via its server-side callback (HandleEventCallbackAsync)
            // while this task is parked here. When stoppingToken is cancelled, OperationCanceledException
            // is thrown, ExecuteAsync returns, and BackgroundService disposes the worker cleanly.
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}