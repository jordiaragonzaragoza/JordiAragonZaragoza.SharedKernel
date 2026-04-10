namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAuthorizationService
    {
        Task<Result> ValidateScopeAsync(string actorId, string actorType, Guid tenantId, Guid? partitionId, Guid? domainId);
    }
}