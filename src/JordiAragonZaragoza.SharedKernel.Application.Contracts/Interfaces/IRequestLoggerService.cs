namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRequestLoggerService
    {
        Task LogRequestAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : notnull;
    }
}