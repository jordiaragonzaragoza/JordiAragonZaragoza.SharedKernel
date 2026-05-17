namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Attributes;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public class RequestAuthorizationService<TRequest> : IRequestAuthorizationService<TRequest>
        where TRequest : notnull
    {
        private readonly IAuthorizationService authorizationService;

        public RequestAuthorizationService(
            IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        public async Task<Result> TryAuthorizeAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var authorizationAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
            if (authorizationAttributes.Count == 0)
            {
                return Result.Success();
            }

            var requiredPermissions = authorizationAttributes
                .SelectMany(authorizationAttribute => authorizationAttribute.Permissions?.Split(',') ?? [])
                .ToList().AsReadOnly();

            var requiredRoles = authorizationAttributes
                .SelectMany(authorizationAttribute => authorizationAttribute.Roles?.Split(',') ?? [])
                .ToList().AsReadOnly();

            var requiredPolicies = authorizationAttributes
                .SelectMany(authorizationAttribute => authorizationAttribute.Policies?.Split(',') ?? [])
                .ToList().AsReadOnly();

            return await this.authorizationService.AuthorizeAsync(
                requiredRoles,
                requiredPermissions,
                requiredPolicies,
                cancellationToken);
        }
    }
}