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
        Task<IEnumerable<TModel>> AddRangeAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default);

        Task<int> UpdateRangeAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default);

        Task<int> DeleteRangeAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
    }
}