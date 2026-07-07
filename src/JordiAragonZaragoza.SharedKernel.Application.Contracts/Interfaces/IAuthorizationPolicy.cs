namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    /// <summary>
    /// A single named authorization policy. Implementations live in the
    /// application layer because they typically need to load a resource
    /// (via a repository) to evaluate ownership or other resource-bound
    /// conditions — this is application orchestration over domain/read
    /// model data, not a domain invariant.
    /// </summary>
    public interface IAuthorizationPolicy
    {
        string Name { get; }

        Task<Result> EvaluateAsync(
            Guid currentUserId,
            IReadOnlyCollection<string> currentUserRoles,
            IReadOnlyCollection<string> currentUserPermissions,
            Guid? resourceId,
            CancellationToken cancellationToken = default);
    }
}