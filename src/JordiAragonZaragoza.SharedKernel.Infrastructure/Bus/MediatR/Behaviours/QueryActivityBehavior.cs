namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class QueryActivityBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IRequestQueryActivityService<TRequest> service;

        public QueryActivityBehavior(IRequestQueryActivityService<TRequest> service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            return this.service.ExecuteWithActivityAsync(
                request,
                _ => next(cancellationToken),
                cancellationToken);
        }
    }
}