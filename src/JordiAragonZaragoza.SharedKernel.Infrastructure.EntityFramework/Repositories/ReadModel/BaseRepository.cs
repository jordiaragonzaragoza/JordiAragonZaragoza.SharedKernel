namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.ReadModel
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;

    public abstract class BaseRepository<TReadModel> : BaseReadRepository<TReadModel>, IRangeableRepository<TReadModel, Guid>
        where TReadModel : class, IReadModel
    {
        protected BaseRepository(BaseReadModelContext dbContext)
            : base(dbContext)
        {
        }
    }
}