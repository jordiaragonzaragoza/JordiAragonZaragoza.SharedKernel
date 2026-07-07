namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    /// <summary>
    /// Evaluates named authorization policies that go beyond static
    /// role/permission checks. Policies receive only the current actor's
    /// identity/roles and an optional resourceId — never the raw request —
    /// keeping the enforcer decoupled from command/query shapes.
    /// </summary>
    public interface IPolicyEnforcer
    {
        /// <summary>
        /// Evaluates a named policy against the current actor's identity/roles and an optional resourceId.
        /// </summary>
        /// <param name="policy">The policy name, e.g. Policies.SelfOrAdmin.</param>
        /// <param name="currentUserId">The authenticated user's id, from ExecutionContext.</param>
        /// <param name="currentUserRoles">The current user's roles within the active scope.</param>
        /// <param name="currentUserPermissions">The current user's permissions within the active scope.</param>
        /// <param name="resourceId">
        /// The id of the resource the policy evaluates ownership against
        /// (e.g. a ReservationId). Null for policies that don't need one.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the policy was satisfied.
        /// </returns>
        Task<Result> AuthorizeAsync(
            string policy,
            Guid currentUserId,
            IReadOnlyCollection<string> currentUserRoles,
            IReadOnlyCollection<string> currentUserPermissions,
            Guid? resourceId,
            CancellationToken cancellationToken = default);
    }
}