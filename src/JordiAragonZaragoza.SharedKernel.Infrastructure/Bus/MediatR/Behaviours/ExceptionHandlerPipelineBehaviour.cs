namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class ExceptionHandlerPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IRequestExceptionHandlerService requestExceptionHandlerService;

        public ExceptionHandlerPipelineBehaviour(
            IRequestExceptionHandlerService requestExceptionHandlerService)
        {
            this.requestExceptionHandlerService = requestExceptionHandlerService ?? throw new ArgumentNullException(nameof(requestExceptionHandlerService));
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => this.requestExceptionHandlerService.ExecuteWithExceptionHandlingAsync(
                request,
                _ => next(cancellationToken),
                cancellationToken);
    }
}