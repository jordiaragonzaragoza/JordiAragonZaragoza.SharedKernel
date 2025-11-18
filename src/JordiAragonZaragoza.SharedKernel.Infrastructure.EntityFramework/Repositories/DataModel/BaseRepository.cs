namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.DataModel
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;

    public abstract class BaseRepository<TDataEntity> : BaseReadRepository<TDataEntity>, IRangeableRepository<TDataEntity, Guid>
        where TDataEntity : class, IDataEntity
    {
        protected BaseRepository(BaseBusinessModelContext dbContext)
            : base(dbContext)
        {
        }
    }
}