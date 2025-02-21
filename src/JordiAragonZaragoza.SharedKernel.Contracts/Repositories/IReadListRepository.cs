namespace JordiAragonZaragoza.SharedKernel.Contracts.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;

    /// <summary>
    /// <para>
    /// A <see cref="IReadListRepository{TModel, TId}" /> can be used to query instances of <typeparamref name="TModel" />.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of model being operated on by this repository.</typeparam>
    /// <typeparam name="TId">The type of id model being operated on by this repository.</typeparam>
    public interface IReadListRepository<TModel, in TId> : IReadRepository<TModel, TId>
        where TModel : class, IBaseModel<TId>
        where TId : notnull
    {
        /// <summary>
        /// Finds all entities of <typeparamref name="TModel" /> from the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        Task<List<TModel>> ListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the total number of records.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// number of elements in the input sequence.
        /// </returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a boolean whether any model exists or not.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains true if the
        /// source sequence contains any elements; otherwise, false.
        /// </returns>
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    }
}