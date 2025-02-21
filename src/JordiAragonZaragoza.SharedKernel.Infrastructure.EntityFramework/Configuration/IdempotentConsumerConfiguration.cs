namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Idempotency;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class IdempotentConsumerConfiguration : BaseModelTypeConfiguration<IdempotentConsumer, Guid>
    {
        public override void Configure(EntityTypeBuilder<IdempotentConsumer> builder)
        {
            _ = builder.ToTable("__IdempotentConsumers");

            base.Configure(builder);
        }
    }
}