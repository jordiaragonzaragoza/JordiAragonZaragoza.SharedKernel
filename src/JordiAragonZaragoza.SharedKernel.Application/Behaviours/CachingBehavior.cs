namespace JordiAragonZaragoza.SharedKernel.Application.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheRequest
        where TResponse : IResult
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> logger;

        public CachingBehavior(
            ICacheService cacheService,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            this.cacheService = Guard.Against.Null(cacheService, nameof(cacheService));
            this.logger = Guard.Against.Null(logger, nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            var cacheKey = request.CacheKey;
            var cachedResponse = await this.cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse.HasValue && !cachedResponse.IsNull)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cachedResponse.Value;
            }

            var response = await next(cancellationToken);

            await this.cacheService.SetAsync(cacheKey, response, request.AbsoluteExpirationInSeconds, cancellationToken);

            this.logger.LogInformation("Set data to cache with cacheKey: {CacheKey}", cacheKey);

            return response;
        }
    }
}