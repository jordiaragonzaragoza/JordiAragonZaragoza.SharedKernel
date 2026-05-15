namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This service is intended to be used in the command and query pipelines, but not when using projections in an event bus pipeline.
    /// Implements the request invalidate caching service that handles cache invalidation logic for requests implementing the IInvalidateCacheRequest interface.
    /// </summary>
    public class RequestInvalidateCachingService : IRequestInvalidateCachingService
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<RequestInvalidateCachingService> logger;

        public RequestInvalidateCachingService(
            ICacheService cacheService,
            ILogger<RequestInvalidateCachingService> logger)
        {
            this.cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> HandleAndInvalidateCacheAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
                where TRequest : IInvalidateCacheRequest
                where TResponse : notnull
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(next);

            var response = await next(cancellationToken);

            var prefixKey = request.PrefixCacheKey;

            await this.cacheService.RemoveByPrefixAsync(prefixKey, cancellationToken);

            this.logger.LogDebug("Cache entries with prefix '{CacheKey}' have been removed.", prefixKey);

            return response;
        }
    }
}