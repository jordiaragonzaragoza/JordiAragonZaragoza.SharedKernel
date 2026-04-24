namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAuthorizationService
    {
        Task<Result> ValidateScopeAsync(Guid userId, ScopeContext scope, CancellationToken cancellationToken = default);
    }
}