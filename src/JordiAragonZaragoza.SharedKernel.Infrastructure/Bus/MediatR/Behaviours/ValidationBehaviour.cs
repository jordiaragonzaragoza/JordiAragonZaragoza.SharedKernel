namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
         where TResponse : IResult
    {
        private readonly IRequestValidationService<TRequest, TResponse> validationHandlerService;

        public ValidationBehaviour(IRequestValidationService<TRequest, TResponse> validationHandlerService)
        {
            this.validationHandlerService = validationHandlerService ?? throw new ArgumentNullException(nameof(validationHandlerService));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            var validationResult = await this.validationHandlerService.TryValidateAsync(request, cancellationToken);
            if (validationResult is not null)
            {
                return validationResult;
            }

            return await next(cancellationToken);
        }
    }
}