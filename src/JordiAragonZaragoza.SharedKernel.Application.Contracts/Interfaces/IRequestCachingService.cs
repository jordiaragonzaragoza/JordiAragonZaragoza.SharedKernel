namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This service is intended to be used in the command and query pipelines, but not when using projections in an event bus pipeline.
    /// Implements the request caching service that handles caching logic for requests implementing the ICacheRequest interface.
    /// </summary>
    public interface IRequestCachingService
    {
        Task<TResponse> HandleWithCacheAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
                where TRequest : ICacheRequest
                where TResponse : notnull;
    }
}