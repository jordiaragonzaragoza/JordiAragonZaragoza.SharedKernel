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

        public Task<Result> SendAsync<T>(T command, CancellationToken cancellationToken = default)
             where T : class, IAsyncCommand
        {
            throw new NotImplementedException();
        }

        /*public async Task<Result> SendAsync<T>(T command, CancellationToken cancellationToken = default)
             where T : class, IAsyncCommand
        {
            ArgumentNullException.ThrowIfNull(command);

            // This transaction is required to commit changes due to use transactional outbox.
            return await this.unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    var asyncCommandHandlerType = AsyncCommandHandlerRegistry.GetAsyncCommandHandlerType(command);
                    var queueName = KebabCaseEndpointNameFormatter.Instance.SanitizeName(asyncCommandHandlerType.Name);

                    var endpoint = await this.sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));

                    try
                    {
                        await endpoint.Send(command, command.GetType(), cancellationToken);

                        this.logger.LogDebug("Sent async command: {Command} {Id} at {DateTime}", command.GetType().Name, command.Id, this.dateTime.UtcNow);
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogError(
                            exception,
                            "Error sending async command: {@Name} {Id} {Content} at {DateTime}",
                            command.GetType().Name,
                            command.Id,
                            command,
                            this.dateTime.UtcNow);

                        throw;
                    }

                    return Result.Success();
                },
                cancellationToken);
        }*/
    }
}