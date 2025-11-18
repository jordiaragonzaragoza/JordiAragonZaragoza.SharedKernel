namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Producers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using Ardalis.Result;
    using global::MassTransit;
    using global::MassTransit.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MassTransitIntegrationCommandBus : IIntegrationCommandBus
    {
        private readonly Bind<IIntegrationBus, IClientFactory> clientFactory;
        private readonly ILogger<MassTransitIntegrationCommandBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationCommandBus(
            Bind<IIntegrationBus, IClientFactory> clientFactory,
            ILogger<MassTransitIntegrationCommandBus> logger,
            IDateTime dateTime)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, IIntegrationCommand
        {
            ArgumentNullException.ThrowIfNull(command);

            var client = this.clientFactory.Value.CreateRequestClient<TCommand>();
            command.DateDispatchedOnUtc = this.dateTime.UtcNow;

            try
            {
                this.logger.LogDebug("Sent Integration command: {Command} {Id}", command.GetType().Name, command.Id);

                var response = await client.GetResponse<Result>(command, cancellationToken);

                return response.Message;
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                   exception,
                   "Error sending integration command: {@Name} {Id} {Content} at {DateTime}",
                   command.GetType().Name,
                   command.Id,
                   command,
                   this.dateTime.UtcNow);

                throw;
            }
        }

        public async Task<Result<TResponse>> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, IIntegrationCommand
            where TResponse : notnull
        {
            ArgumentNullException.ThrowIfNull(command);

            var client = this.clientFactory.Value.CreateRequestClient<TCommand>();
            command.DateDispatchedOnUtc = this.dateTime.UtcNow;

            try
            {
                this.logger.LogDebug("Sent Integration command: {Command} {Id}", command.GetType().Name, command.Id);

                var response = await client.GetResponse<Result<TResponse>>(command, cancellationToken);

                return response.Message;
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                   exception,
                   "Error sending integration command: {@Name} {Id} {Content} at {DateTime}",
                   command.GetType().Name,
                   command.Id,
                   command,
                   this.dateTime.UtcNow);

                throw;
            }
        }
    }
}