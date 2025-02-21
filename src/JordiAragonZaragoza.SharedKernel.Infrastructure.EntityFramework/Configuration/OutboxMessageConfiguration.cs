namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Outbox;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OutboxMessageConfiguration : BaseModelTypeConfiguration<OutboxMessage, Guid>
    {
        public override void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            _ = builder.ToTable("__OutboxMessages");

            base.Configure(builder);

            _ = builder.Property(static outboxMessage => outboxMessage.Version)
                .IsRowVersion();
        }
    }
}