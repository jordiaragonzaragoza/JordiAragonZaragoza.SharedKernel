namespace JordiAragonZaragoza.SharedKernel.Application.Behaviours
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Attributes;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MediatR;

    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly ICurrentUserService currentUserService;
        private readonly IIdentityService identityService;

        public AuthorizationBehaviour(
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            this.currentUserService = currentUserService;
            this.identityService = identityService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();

            if (authorizeAttributes.Count > 0)
            {
                // Must be authenticated user
                if (this.currentUserService.UserId == null)
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
                            var isInRole = await this.identityService.IsInRoleAsync(this.currentUserService.UserId, role.Trim());
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
                        var authorized = await this.identityService.AuthorizeAsync(this.currentUserService.UserId, policy);

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

            // User is authorized / authorization not required
            return await next();
        }
    }
}