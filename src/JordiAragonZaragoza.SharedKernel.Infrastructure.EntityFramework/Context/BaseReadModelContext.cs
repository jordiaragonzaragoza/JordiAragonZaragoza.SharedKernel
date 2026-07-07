namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BaseReadModelContext : BaseContext
    {
        private readonly TenantReadModelSaveChangesInterceptor tenantInterceptor;

        protected BaseReadModelContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IHostEnvironment hostEnvironment,
            TenantReadModelSaveChangesInterceptor tenantInterceptor)
            : base(options, loggerFactory, hostEnvironment)
        {
            this.tenantInterceptor = tenantInterceptor
                ?? throw new ArgumentNullException(nameof(tenantInterceptor));
        }

        public Guid? CurrentTenantId { get; set; }

        public DbSet<Checkpoint> Checkpoints => this.Set<Checkpoint>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            _ = optionsBuilder.AddInterceptors(this.tenantInterceptor);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

            _ = modelBuilder.ApplyConfiguration(new CheckpointConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}