namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CheckpointConfiguration : IEntityTypeConfiguration<Checkpoint>
    {
        public void Configure(EntityTypeBuilder<Checkpoint> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            _ = builder.ToTable("__Checkpoints");

            _ = builder.HasKey(static ckeckpoint => ckeckpoint.Id);
        }
    }
}