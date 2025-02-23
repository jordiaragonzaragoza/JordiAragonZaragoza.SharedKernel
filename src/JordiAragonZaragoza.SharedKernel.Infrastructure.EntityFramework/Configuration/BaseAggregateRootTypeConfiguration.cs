namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public abstract class BaseAggregateRootTypeConfiguration<TAggregateRoot, TId> : BaseModelTypeConfiguration<TAggregateRoot, TId>
        where TAggregateRoot : class, IAggregateRoot<TId>
        where TId : class, IEntityId
    {
        public override void Configure(EntityTypeBuilder<TAggregateRoot> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            base.Configure(builder);

            _ = builder.Property(static aggregateRoot => aggregateRoot.Version)
                .IsRowVersion();

            _ = builder.Property<bool>("IsDeleted");
            _ = builder.HasQueryFilter(static b => !EF.Property<bool>(b, "IsDeleted"));
        }
    }
}