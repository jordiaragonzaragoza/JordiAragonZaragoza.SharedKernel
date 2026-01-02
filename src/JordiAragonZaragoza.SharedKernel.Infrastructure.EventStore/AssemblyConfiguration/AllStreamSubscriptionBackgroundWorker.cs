namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class AllStreamSubscriptionBackgroundWorker : BackgroundService
    {
        private readonly ILogger<AllStreamSubscriptionBackgroundWorker> logger;
        private readonly Func<CancellationToken, Task> perform;

        public AllStreamSubscriptionBackgroundWorker(
            ILogger<AllStreamSubscriptionBackgroundWorker> logger,
            Func<CancellationToken, Task> perform)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.perform = perform ?? throw new ArgumentNullException(nameof(perform));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(
                        async () =>
                        {
                            await Task.Yield();
                            this.logger.LogInformation("All Stream Subscription Background worker started");

                            await this.perform(stoppingToken).ConfigureAwait(false);

                            this.logger.LogInformation("All Stream Subscription Background worker stopped");
                        },
                        stoppingToken);
        }
    }
}