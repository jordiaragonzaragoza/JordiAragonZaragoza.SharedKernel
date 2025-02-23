namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.BusinessModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Specification.EntityFrameworkCore;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Entities;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;

    public abstract class BaseReadRepository<TEntity, TId> : RepositoryBase<TEntity>, ISpecificationReadRepository<TEntity, TId>, IScopedDependency
        where TEntity : class, IEntity<TId>
        where TId : class, IEntityId
    {
        protected BaseReadRepository(BaseBusinessModelContext readContext)
            : base(readContext)
        {
        }

        public virtual Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return this.FirstOrDefaultAsync(new EntityByIdSpec<TEntity, TId>(id), cancellationToken);
        }

        [Obsolete("This method is obsolete. Use GetByIdAsync instead.")]
        public new Task<TEntity?> GetByIdAsync<TIdx>(TIdx id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This method is obsolete. Use GetByIdAsync instead.");
        }
    }
}