namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IRequestAuthorizationService<TRequest> authorizationHandlerService;

        public AuthorizationBehaviour(IRequestAuthorizationService<TRequest> authorizationHandlerService)
        {
            this.authorizationHandlerService = authorizationHandlerService ?? throw new ArgumentNullException(nameof(authorizationHandlerService));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            var authorizationResult = await this.authorizationHandlerService.TryAuthorizeAsync(request, cancellationToken);

            if (!authorizationResult.IsSuccess)
            {
                return ConvertResultToTResponse(authorizationResult);
            }

            return await next(cancellationToken);
        }

        private static TResponse ConvertResultToTResponse(Result authorizationResult)
        {
            var methodName = authorizationResult.Status.ToString();

            // Try to find a static method on TResponse that matches the status name and accepts a string parameter (for error message)
            var resultMethod = typeof(TResponse).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public,
                null,
                [typeof(string)],
                null);

            if (resultMethod != null)
            {
                var errorMessage = authorizationResult.Errors?.FirstOrDefault()
                    ?? authorizationResult.ToString();

                var result = resultMethod.Invoke(null, [errorMessage])
                    ?? throw new InvalidOperationException(
                        $"The '{methodName}' method returned null for type {typeof(TResponse).FullName}");

                return (TResponse)result;
            }

            // If it doesn't find a method with a string parameter, try without parameters
            resultMethod = typeof(TResponse).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                null)
                ?? throw new InvalidOperationException(
                    $"The '{methodName}' method was not found on type {typeof(TResponse).FullName}");

            var resultWithoutParams = resultMethod.Invoke(null, null)
                ?? throw new InvalidOperationException(
                    $"The '{methodName}' method returned null for type {typeof(TResponse).FullName}");

            return (TResponse)resultWithoutParams;
        }
    }
}