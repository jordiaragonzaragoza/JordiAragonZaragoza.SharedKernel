namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.DataModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Specification.EntityFrameworkCore;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;

    public abstract class BaseReadRepository<TDataEntity> : RepositoryBase<TDataEntity>, ISpecificationReadRepository<TDataEntity, Guid>
        where TDataEntity : class, IDataEntity
    {
        protected BaseReadRepository(BaseBusinessModelContext readContext)
            : base(readContext)
        {
        }

        public virtual Task<TDataEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return this.GetByIdAsync<Guid>(id, cancellationToken);
        }
    }
}