namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IRequestValidationService<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        Task<TResponse?> TryValidateAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}