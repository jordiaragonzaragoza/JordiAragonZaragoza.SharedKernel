namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public class PolicyEnforcer : IPolicyEnforcer
    {
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        private readonly IReadOnlyDictionary<string, IAuthorizationPolicy> policies;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        public PolicyEnforcer(IEnumerable<IAuthorizationPolicy> policies)
        {
            ArgumentNullException.ThrowIfNull(policies);
            this.policies = policies.ToDictionary(p => p.Name, StringComparer.Ordinal);
        }

        public async Task<Result> AuthorizeAsync(
            string policy,
            Guid currentUserId,
            IReadOnlyCollection<string> currentUserRoles,
            IReadOnlyCollection<string> currentUserPermissions,
            Guid? resourceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(policy);

            if (!this.policies.TryGetValue(policy, out var authorizationPolicy))
            {
                throw new InvalidOperationException(
                    $"Policy '{policy}' is declared via [Authorize] but has no " +
                    $"registered IAuthorizationPolicy implementation.");
            }

            return await authorizationPolicy.EvaluateAsync(
                currentUserId,
                currentUserRoles,
                currentUserPermissions,
                resourceId,
                cancellationToken);
        }
    }
}