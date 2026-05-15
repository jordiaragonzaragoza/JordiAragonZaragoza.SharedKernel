namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This service is intended to be used in the command and query pipelines, but not when using projections in an event bus pipeline.
    /// Implements the request invalidate caching service that handles cache invalidation logic for requests implementing the IInvalidateCacheRequest interface.
    /// </summary>
    public interface IRequestInvalidateCachingService
    {
        Task<TResponse> HandleAndInvalidateCacheAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
                where TRequest : IInvalidateCacheRequest
                where TResponse : notnull;
    }
}