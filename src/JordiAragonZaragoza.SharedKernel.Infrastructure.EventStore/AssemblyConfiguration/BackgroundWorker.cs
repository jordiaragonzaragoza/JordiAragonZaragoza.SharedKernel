namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class BackgroundWorker : BackgroundService, IIgnoreDependency
    {
        private readonly ILogger<BackgroundWorker> logger;
        private readonly Func<CancellationToken, Task> perform;

        public BackgroundWorker(
            ILogger<BackgroundWorker> logger,
            Func<CancellationToken, Task> perform)
        {
            this.logger = Guard.Against.Null(logger, nameof(logger));
            this.perform = Guard.Against.Null(perform, nameof(perform));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(
                        async () =>
                        {
                            await Task.Yield();
                            this.logger.LogInformation("Background worker started");

                            await this.perform(stoppingToken).ConfigureAwait(false);

                            this.logger.LogInformation("Background worker stopped");
                        },
                        stoppingToken);
        }
    }
}