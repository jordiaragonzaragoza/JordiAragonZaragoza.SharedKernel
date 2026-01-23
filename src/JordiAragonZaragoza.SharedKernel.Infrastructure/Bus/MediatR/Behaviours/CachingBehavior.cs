namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

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