namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IRequestValidationService<TRequest>
        where TRequest : notnull
    {
        Task<Result> TryValidateAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}