namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class AllStreamSubscriptionBackgroundWorker : BackgroundService
    {
        private const double BackoffMultiplier = 2.0;
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);

        private readonly ILogger<AllStreamSubscriptionBackgroundWorker> logger;
        private readonly Func<CancellationToken, Task> perform;

        public AllStreamSubscriptionBackgroundWorker(
            ILogger<AllStreamSubscriptionBackgroundWorker> logger,
            Func<CancellationToken, Task> perform)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.perform = perform ?? throw new ArgumentNullException(nameof(perform));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Yield immediately so BackgroundService.StartAsync returns promptly
            // and does not block host startup while we process catch-up history.
            await Task.Yield();

            this.logger.LogInformation("All Stream Subscription Background Worker started");

            await this.RunWithRetryAsync(stoppingToken).ConfigureAwait(false);

            this.logger.LogInformation("All Stream Subscription Background Worker stopped");
        }

        private static TimeSpan CalculateDelay(int attempt)
        {
            // Exponential backoff: 2s, 4s, 8s, 16s, 32s, 60s (capped)
            var exponential = InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attempt - 1);
            var capped = Math.Min(exponential, MaxDelay.TotalMilliseconds);

    #pragma warning disable CA5394
            // Add jitter: ±20% to avoid thundering herd
            var jitter = capped * 0.2 * (Random.Shared.NextDouble() - 0.5);
    #pragma warning restore CA5394

            return TimeSpan.FromMilliseconds(capped + jitter);
        }

        private async Task RunWithRetryAsync(CancellationToken stoppingToken)
        {
            var attempt = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
    #pragma warning disable CA1031
                try
                {
                    if (attempt > 0)
                    {
                        var delay = CalculateDelay(attempt);
                        this.logger.LogWarning(
                            "Reconnecting subscription (attempt {Attempt}) after {Delay:F1}s...",
                            attempt,
                            delay.TotalSeconds);

                        await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
                    }

                    await this.perform(stoppingToken).ConfigureAwait(false);

                    // perform returned without exception: KurrentDB closed the connection cleanly.
                    // Reset attempt counter and reconnect immediately.
                    attempt = 0;
                    this.logger.LogWarning(
                        "Subscription ended without exception. Reconnecting...");
                }
                catch (OperationCanceledException operationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    this.logger.LogInformation(
                        operationCanceledException,
                        "All Stream Subscription Background Worker stopping due to host shutdown");
                    return;
                }
                catch (Exception exception)
                {
                    attempt++;
                    var delay = CalculateDelay(attempt);

                    this.logger.LogError(
                        exception,
                        "Subscription error on attempt {Attempt}. Retrying in {Delay:F1}s",
                        attempt,
                        delay.TotalSeconds);
                }
    #pragma warning restore CA1031
            }
        }
    }
}