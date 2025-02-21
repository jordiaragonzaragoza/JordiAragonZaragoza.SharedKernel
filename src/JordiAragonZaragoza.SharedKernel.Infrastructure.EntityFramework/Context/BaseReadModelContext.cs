namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BaseReadModelContext : BaseContext
    {
        protected BaseReadModelContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IHostEnvironment hostEnvironment)
            : base(options, loggerFactory, hostEnvironment)
        {
        }

        public DbSet<Checkpoint> Checkpoints => this.Set<Checkpoint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

            _ = modelBuilder.ApplyConfiguration(new CheckpointConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}