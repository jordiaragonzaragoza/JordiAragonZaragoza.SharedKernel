namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class RequestCachingService : IRequestCachingService
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<RequestCachingService> logger;

        public RequestCachingService(
            ICacheService cacheService,
            ILogger<RequestCachingService> logger)
        {
            this.cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> HandleWithCacheAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
                where TRequest : ICacheRequest
                where TResponse : notnull
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(next);

            var cacheKey = request.CacheKey;

            var cachedResponse = await this.cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse.HasValue && !cachedResponse.IsNull)
            {
                this.logger.LogDebug("Fetched data from cache with cacheKey: {CacheKey}", cacheKey);
                return cachedResponse.Value;
            }

            var response = await next(cancellationToken);

            await this.cacheService.SetAsync(cacheKey, response, request.AbsoluteExpirationInSeconds, cancellationToken);
            this.logger.LogDebug("Set data to cache with cacheKey: {CacheKey}", cacheKey);

            return response;
        }
    }
}