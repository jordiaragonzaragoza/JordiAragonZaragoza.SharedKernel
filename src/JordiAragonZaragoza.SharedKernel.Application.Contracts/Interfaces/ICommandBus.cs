namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface ICommandBus
    {
        Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default);

        Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
            where TResponse : notnull;
    }
}