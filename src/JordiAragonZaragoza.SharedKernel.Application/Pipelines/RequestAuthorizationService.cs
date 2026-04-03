namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Attributes;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;

    public class RequestAuthorizationService<TRequest, TResponse> : IRequestAuthorizationService<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        private readonly IExecutionContextService executionContextService;
        private readonly IIdentityService identityService;

        public RequestAuthorizationService(
            IExecutionContextService executionContextService,
            IIdentityService identityService)
        {
            this.executionContextService = executionContextService ?? throw new ArgumentNullException(nameof(executionContextService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        public async Task<TResponse?> TryAuthorizeAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
            var actorId = this.executionContextService.CurrentContext.ActorId;
            if (authorizeAttributes.Count > 0)
            {
                // Must be authenticated user
                if (actorId == ActorConstants.Anonymous)
                {
                    // Get Ardalis.Result.Unauthorized or Ardalis.Result<T>.Unauthorized method.
                    var resultUnauthorizedMethod = typeof(TResponse).GetMethod("Unauthorized", BindingFlags.Static | BindingFlags.Public)
                        ?? throw new InvalidOperationException("The 'Unauthorized' method was not found on type " + typeof(TResponse).FullName);

                    var result = resultUnauthorizedMethod.Invoke(null, null)
                        ?? throw new InvalidOperationException("The 'Unauthorized' method returned null.");

                    return (TResponse)result;

                    ////throw new UnauthorizedAccessException();
                }

                // Role-based authorization
                var authorizeAttributesWithRoles = authorizeAttributes.Where(static a => !string.IsNullOrWhiteSpace(a.Roles)).ToList();

                if (authorizeAttributesWithRoles.Count > 0)
                {
                    var authorized = false;

                    foreach (var roles in authorizeAttributesWithRoles.Select(static a => a.Roles.Split(',')))
                    {
                        foreach (var role in roles)
                        {
                            var isInRole = await this.identityService.IsInRoleAsync(actorId, role.Trim());
                            if (isInRole)
                            {
                                authorized = true;
                                break;
                            }
                        }
                    }

                    // Must be a member of at least one role in roles
                    if (!authorized)
                    {
                        // Get Ardalis.Result.Forbidden or Ardalis.Result<T>.Forbidden method.
                        var resultForbiddenMethod = typeof(TResponse).GetMethod("Forbidden", BindingFlags.Static | BindingFlags.Public)
                            ?? throw new InvalidOperationException("The 'Forbidden' method was not found on type " + typeof(TResponse).FullName);

                        var result = resultForbiddenMethod.Invoke(null, null)
                            ?? throw new InvalidOperationException("The 'Forbidden' method returned null.");

                        return (TResponse)result;
                    }
                }

                // Policy-based authorization
                var authorizeAttributesWithPolicies = authorizeAttributes.Where(static a => !string.IsNullOrWhiteSpace(a.Policy)).ToList();
                if (authorizeAttributesWithPolicies.Count > 0)
                {
                    foreach (var policy in authorizeAttributesWithPolicies.Select(static a => a.Policy))
                    {
                        var authorized = await this.identityService.AuthorizeAsync(actorId, policy);

                        if (!authorized)
                        {
                            // Get Ardalis.Result.Forbidden or Ardalis.Result<T>.Forbidden method.
                            var resultForbiddenMethod = typeof(TResponse).GetMethod("Forbidden", BindingFlags.Static | BindingFlags.Public)
                            ?? throw new InvalidOperationException("The 'Forbidden' method was not found on type " + typeof(TResponse).FullName);

                            var result = resultForbiddenMethod.Invoke(null, null)
                            ?? throw new InvalidOperationException("The 'Forbidden' method returned null.");

                            return (TResponse)result;
                        }
                    }
                }
            }

            return default;
        }
    }
}