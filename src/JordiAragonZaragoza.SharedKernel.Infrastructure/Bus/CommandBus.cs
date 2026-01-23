namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class CommandBus : ICommandBus
    {
        private readonly ISender sender;

        public CommandBus(
            ISender sender)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            return this.sender.Send(command, cancellationToken);
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
            where TResponse : notnull
        {
            return this.sender.Send(command, cancellationToken);
        }
    }
}