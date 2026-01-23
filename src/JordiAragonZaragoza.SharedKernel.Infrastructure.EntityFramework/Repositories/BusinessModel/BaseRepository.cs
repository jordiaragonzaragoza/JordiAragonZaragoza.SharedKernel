namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.BusinessModel
{
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Entities;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;

    public abstract class BaseRepository<TAggregate, TId> : BaseReadRepository<TAggregate, TId>, IRangeableRepository<TAggregate, TId>
        where TAggregate : BaseAggregateRoot<TId>
        where TId : class, IEntityId
    {
        protected BaseRepository(BaseBusinessModelContext dbContext)
            : base(dbContext)
        {
        }
    }
}