namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IRequestAuthorizationService<TRequest>
        where TRequest : notnull
    {
        Task<Result> TryAuthorizeAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}