namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IRequestAuthorizationService<TRequest, TResponse> authorizationHandlerService;

        public AuthorizationBehaviour(IRequestAuthorizationService<TRequest, TResponse> authorizationHandlerService)
        {
            this.authorizationHandlerService = authorizationHandlerService ?? throw new ArgumentNullException(nameof(authorizationHandlerService));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            var authorizationResult = await this.authorizationHandlerService.TryAuthorizeAsync(request, cancellationToken);

            if (authorizationResult is not null)
            {
                return authorizationResult;
            }

            return await next(cancellationToken);
        }
    }
}