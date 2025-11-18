namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MassTransit;

    public abstract class BaseAsyncCommandHandler<TAsyncCommand> : IAsyncCommandHandler<TAsyncCommand>
        where TAsyncCommand : class, IAsyncCommand
    {
        public async Task Consume(ConsumeContext<TAsyncCommand> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            await this.HandleAsync(context.Message, CancellationToken.None);
        }

        public abstract Task HandleAsync(TAsyncCommand asyncCommand, CancellationToken cancellationToken);
    }
}