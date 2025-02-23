namespace JordiAragonZaragoza.SharedKernel.Contracts.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Specification;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;

    /// <summary>
    /// <para>
    /// A <see cref="ISpecificationReadRepository{TModel, TId}" /> can be used to query instances of <typeparamref name="TModel" />.
    /// An <see cref="ISpecification{TModel}"/> (or derived) is used to encapsulate the LINQ queries against the database.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of model being operated on by this repository.</typeparam>
    /// <typeparam name="TId">The type of id model being operated on by this repository.</typeparam>
    public interface ISpecificationReadRepository<TModel, in TId> : IReadListRepository<TModel, TId>
        where TModel : class, IBaseModel<TId>
        where TId : notnull
    {
        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <typeparamref name="TModel" />, or <see langword="null"/>.
        /// </returns>
        Task<TModel?> FirstOrDefaultAsync(ISpecification<TModel> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <typeparamref name="TResult" />, or <see langword="null"/>.
        /// </returns>
        Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TModel, TResult> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <typeparamref name="TModel" />, or <see langword="null"/>.
        /// </returns>
        Task<TModel?> SingleOrDefaultAsync(ISingleResultSpecification<TModel> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <typeparamref name="TResult" />, or <see langword="null"/>.
        /// </returns>
        Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TModel, TResult> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all entities of <typeparamref name="TModel" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        Task<List<TModel>> ListAsync(ISpecification<TModel> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all entities of <typeparamref name="TModel" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// <para>
        /// Projects each model into a new form, being <typeparamref name="TResult" />.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the projection.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{TResult}" /> that contains elements from the input sequence.
        /// </returns>
        Task<List<TResult>> ListAsync<TResult>(ISpecification<TModel, TResult> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a number that represents how many entities satisfy the encapsulated query logic
        /// of the <paramref name="specification"/>.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// number of elements in the input sequence.
        /// </returns>
        Task<int> CountAsync(ISpecification<TModel> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a boolean that represents whether any model satisfy the encapsulated query logic
        /// of the <paramref name="specification"/> or not.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains true if the
        /// source sequence contains any elements; otherwise, false.
        /// </returns>
        Task<bool> AnyAsync(ISpecification<TModel> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all entities of <typeparamref name="TModel" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>
        ///  Returns an IAsyncEnumerable which can be enumerated asynchronously.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Ok to use Ardalis.Specification.IReadRepositoryBase")]
        IAsyncEnumerable<TModel> AsAsyncEnumerable(ISpecification<TModel> specification);
    }
}