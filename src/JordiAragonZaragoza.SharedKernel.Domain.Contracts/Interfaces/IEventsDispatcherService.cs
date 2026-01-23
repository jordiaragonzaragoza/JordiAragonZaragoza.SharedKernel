namespace JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a contract for dispatching domain events collected during the execution of application logic.
    /// </summary>
    public interface IEventsDispatcherService
    {
        /// <summary>
        /// Dispatches in domain events stored in aggregates store.
        /// This should be called just before committing the transaction when using the Unit of Work pattern.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DispatchEventsFromAggregatesStoreAsync(CancellationToken cancellationToken = default);
    }
}