namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;

    public interface IAsyncCommandHandler<in TAsyncCommand> : IConsumer<TAsyncCommand>, IAsyncCommandHandler
        where TAsyncCommand : class, IAsyncCommand
    {
        Task HandleAsync(TAsyncCommand asyncCommand, CancellationToken cancellationToken);
    }
}