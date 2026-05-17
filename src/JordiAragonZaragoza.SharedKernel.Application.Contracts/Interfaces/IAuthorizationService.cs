namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAuthorizationService
    {
        Task<Result> ValidateScopeAsync(Guid userId, ScopeContext scope, CancellationToken cancellationToken = default);

        Task<Result> AuthorizeAsync(
            ReadOnlyCollection<string> requiredRoles,
            ReadOnlyCollection<string> requiredPermissions,
            ReadOnlyCollection<string> requiredPolicies,
            CancellationToken cancellationToken = default);
    }
}