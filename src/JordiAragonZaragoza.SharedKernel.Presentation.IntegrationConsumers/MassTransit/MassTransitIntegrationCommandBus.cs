namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MassTransit;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class MassTransitIntegrationCommandBus : IIntegrationCommandBus, IScopedDependency
    {
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly ILogger<MassTransitIntegrationCommandBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationCommandBus(
            ISendEndpointProvider sendEndpointProvider,
            ILogger<MassTransitIntegrationCommandBus> logger,
            IDateTime dateTime)
        {
            this.sendEndpointProvider = sendEndpointProvider ?? throw new ArgumentNullException(nameof(sendEndpointProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task SendCommandAsync<T>(T command, Uri endpointAddress, CancellationToken cancellationToken = default)
            where T : class, IIntegrationCommand
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            var endpoint = await this.sendEndpointProvider.GetSendEndpoint(endpointAddress);

            command.DateDispatchedOnUtc = this.dateTime.UtcNow;

            await endpoint.Send(command, cancellationToken);

            this.logger.LogInformation("Sent Integration command: {Command} {Id}", command.GetType().Name, command.Id);
        }
    }
}