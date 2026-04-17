namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAuthorizationService
    {
        Task<Result> ValidateScopeAsync(ExecutionContext executionContext);
    }
}