namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks that the user has at least one assignment covering the scope.
        /// </summary>
        /// <param name="userId">The user to check.</param>
        /// <param name="scope">The requested scope (tenant, partition, cinema).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Success when the user can act in the scope; Forbidden otherwise.</returns>
        Task<Result> ValidateScopeAsync(Guid userId, ScopeContext scope, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authorizes the current actor against the requirements declared by the [Authorize] attributes.
        /// All the requirements must be satisfied (one requirement per attribute).
        /// </summary>
        /// <param name="requirements">One requirement per [Authorize] attribute.</param>
        /// <param name="resourceId">The resource the request refers to, used by policies.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Success when every requirement is satisfied; otherwise the failing result.</returns>
        Task<Result> AuthorizeAsync(
            IReadOnlyCollection<AuthorizationRequirement> requirements,
            Guid? resourceId,
            CancellationToken cancellationToken = default);
    }
}
