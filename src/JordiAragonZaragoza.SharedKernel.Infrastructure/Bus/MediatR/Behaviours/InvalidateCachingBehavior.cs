namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

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