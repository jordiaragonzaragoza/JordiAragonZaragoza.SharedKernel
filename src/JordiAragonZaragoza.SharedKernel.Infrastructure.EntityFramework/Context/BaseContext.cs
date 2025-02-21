namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context
{
    using System;
    using Ardalis.GuardClauses;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SmartEnum.EFCore;

    public abstract class BaseContext : DbContext
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "It is ok for ILoggerFactory is managed by dependency inyection.")]
        private readonly ILoggerFactory loggerFactory;
        private readonly IHostEnvironment hostEnvironment;

        protected BaseContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IHostEnvironment hostEnvironment)
            : base(options)
        {
            this.loggerFactory = Guard.Against.Null(loggerFactory, nameof(loggerFactory));
            this.hostEnvironment = Guard.Against.Null(hostEnvironment, nameof(hostEnvironment));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            _ = optionsBuilder
                .UseLoggerFactory(this.loggerFactory)
                .EnableSensitiveDataLogging(this.hostEnvironment.EnvironmentName == "Development")
                .EnableDetailedErrors(this.hostEnvironment.EnvironmentName == "Development");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureSmartEnum();

            base.ConfigureConventions(configurationBuilder);
        }
    }
}