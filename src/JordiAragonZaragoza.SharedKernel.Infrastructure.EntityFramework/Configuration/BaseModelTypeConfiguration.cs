namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public abstract class BaseModelTypeConfiguration<TModel, TId> : IEntityTypeConfiguration<TModel>
        where TModel : class, IBaseModel<TId>
        where TId : notnull
    {
        public virtual void Configure(EntityTypeBuilder<TModel> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            _ = builder.HasKey(static x => x.Id);
        }
    }
}