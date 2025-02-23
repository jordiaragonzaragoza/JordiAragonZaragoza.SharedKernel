namespace JordiAragonZaragoza.SharedKernel.Domain.Entities
{
    using System;
    using Ardalis.Specification;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public sealed class EntityByIdSpec<TEntity, TId> : SingleResultSpecification<TEntity>
        where TEntity : class, JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces.IEntity<TId>
        where TId : class, IEntityId
    {
        public EntityByIdSpec(TId entityId)
        {
            ArgumentNullException.ThrowIfNull(entityId);

            _ = this.Query
                    .Where(entity => entity.Id == entityId);
        }
    }
}