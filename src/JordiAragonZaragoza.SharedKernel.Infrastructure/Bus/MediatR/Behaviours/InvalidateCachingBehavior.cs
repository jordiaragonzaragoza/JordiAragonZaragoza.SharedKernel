namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This service is intended to be used in the command and query pipelines, but not when using projections in an event bus pipeline.
    /// Implements the invalidate caching behavior that handles cache invalidation logic for requests implementing the IInvalidateCacheRequest interface.
    /// </summary>
    /// <typeparam name="TRequest"> the request type. </typeparam>
    /// <typeparam name="TResponse"> the response type. </typeparam>
    public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IInvalidateCacheRequest
        where TResponse : IResult
    {
        private readonly IRequestInvalidateCachingService requestInvalidateCachingService;

        public InvalidateCachingBehavior(IRequestInvalidateCachingService requestInvalidateCachingService)
        {
            this.requestInvalidateCachingService = requestInvalidateCachingService ?? throw new ArgumentNullException(nameof(requestInvalidateCachingService));
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
            => this.requestInvalidateCachingService.HandleAndInvalidateCacheAsync(
                request,
                _ => next(cancellationToken),
                cancellationToken);
    }
}