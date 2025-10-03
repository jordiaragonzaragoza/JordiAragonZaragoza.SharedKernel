namespace JordiAragonZaragoza.SharedKernel.Contracts.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;

    public interface IRangeableRepository<TModel, in TId> : IRepository<TModel, TId>
        where TModel : class, IBaseModel<TId>
        where TId : notnull
    {
        Task<IEnumerable<TModel>> AddRangeAsync(IEnumerable<TModel> entities, CancellationToken cancellationToken = default);

        Task<int> UpdateRangeAsync(IEnumerable<TModel> entities, CancellationToken cancellationToken = default);

        Task<int> DeleteRangeAsync(IEnumerable<TModel> aggregates, CancellationToken cancellationToken = default);
    }
}