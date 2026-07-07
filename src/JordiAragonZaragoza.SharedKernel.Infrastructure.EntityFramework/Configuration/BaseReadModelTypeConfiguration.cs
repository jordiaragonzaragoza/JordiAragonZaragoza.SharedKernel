namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public abstract class BaseReadModelTypeConfiguration<TReadModel, TId, TContext>
        : BaseModelTypeConfiguration<TReadModel, TId>
        where TReadModel : class, IReadModel, IBaseModel<TId>
        where TId : notnull
        where TContext : BaseReadModelContext
    {
        private readonly TContext dbContext;

        protected BaseReadModelTypeConfiguration(TContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public override void Configure(EntityTypeBuilder<TReadModel> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            base.Configure(builder);

            builder.OwnsOne(x => x.Scope, scopeBuilder =>
            {
                scopeBuilder.Property(s => s.TenantId)
                    .HasColumnName("Scope_TenantId")
                    .IsRequired();

                scopeBuilder.Property(s => s.PartitionId)
                    .HasColumnName("Scope_PartitionId");

                scopeBuilder.Property(s => s.DomainId)
                    .HasColumnName("Scope_DomainId");
            });

            // Query filter uses only TenantId for automatic row-level security.
            // PartitionId/DomainId are additional access filters applied via
            // specifications when relevant — not as global query filters —
            // because their semantics depend on the data model of each read model.
            builder.HasQueryFilter(m =>
                this.dbContext.CurrentTenantId == null
                || m.Scope.TenantId == this.dbContext.CurrentTenantId);
        }
    }
}