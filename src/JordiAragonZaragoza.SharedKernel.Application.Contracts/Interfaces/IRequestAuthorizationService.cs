namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IRequestAuthorizationService<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        Task<TResponse?> TryAuthorizeAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}