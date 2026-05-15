namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    // <summary>
    // ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    // This service is intended to be used in the command and query pipelines, but not when using projections in an event bus pipeline.
    // Implements the caching behavior that handles caching logic for requests implementing the ICacheRequest interface.
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheRequest
        where TResponse : IResult
    {
        private readonly IRequestCachingService cacheRequestHandlerService;

        public CachingBehavior(IRequestCachingService cacheRequestHandlerService)
        {
            this.cacheRequestHandlerService = cacheRequestHandlerService ?? throw new ArgumentNullException(nameof(cacheRequestHandlerService));
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
            => this.cacheRequestHandlerService.HandleWithCacheAsync(
                request,
                _ => next(cancellationToken),
                cancellationToken);
    }
}