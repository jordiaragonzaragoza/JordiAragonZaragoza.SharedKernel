namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IAsyncCommandBus
    {
        // TODO: Complete.
        /*Task<Result> SendAsync(IAsyncCommand command, CancellationToken cancellationToken = default);

        Task<Result<TResponse>> SendAsync<TResponse>(IAsyncCommand<TResponse> command, CancellationToken cancellationToken = default)
            where TResponse : notnull;*/
    }
}