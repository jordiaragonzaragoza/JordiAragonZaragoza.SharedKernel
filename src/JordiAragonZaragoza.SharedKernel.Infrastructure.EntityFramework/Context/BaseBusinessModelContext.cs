namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context
{
    using System;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Idempotency;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Outbox;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BaseBusinessModelContext : BaseContext
    {
        private readonly SoftDeleteEntitySaveChangesInterceptor softDeleteEntitySaveChangesInterceptor;

        protected BaseBusinessModelContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IHostEnvironment hostEnvironment,
            SoftDeleteEntitySaveChangesInterceptor softDeleteEntitySaveChangesInterceptor)
            : base(options, loggerFactory, hostEnvironment)
        {
            this.softDeleteEntitySaveChangesInterceptor = Guard.Against.Null(softDeleteEntitySaveChangesInterceptor, nameof(softDeleteEntitySaveChangesInterceptor));
        }

        public DbSet<OutboxMessage> OutboxMessages => this.Set<OutboxMessage>();

        public DbSet<IdempotentConsumer> IdempotentConsumers => this.Set<IdempotentConsumer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

            _ = modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            _ = modelBuilder.ApplyConfiguration(new IdempotentConsumerConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            _ = optionsBuilder.AddInterceptors(this.softDeleteEntitySaveChangesInterceptor);

            base.OnConfiguring(optionsBuilder);
        }
    }
}