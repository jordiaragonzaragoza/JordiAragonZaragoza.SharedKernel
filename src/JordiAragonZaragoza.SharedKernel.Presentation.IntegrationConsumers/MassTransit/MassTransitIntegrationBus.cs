namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MassTransit;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class MassTransitIntegrationBus : IIntegrationBus, IScopedDependency
    {
        private readonly IPublishEndpoint publishEndPoint;
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly ILogger<MassTransitIntegrationBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationBus(
            IPublishEndpoint publishEndPoint,
            ISendEndpointProvider sendEndpointProvider,
            ILogger<MassTransitIntegrationBus> logger,
            IDateTime dateTime)
        {
            this.publishEndPoint = publishEndPoint ?? throw new ArgumentNullException(nameof(publishEndPoint));
            this.sendEndpointProvider = sendEndpointProvider ?? throw new ArgumentNullException(nameof(sendEndpointProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IIntegrationEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            @event.DatePublishedOnUtc = this.dateTime.UtcNow;

            await this.publishEndPoint.Publish(@event, cancellationToken);

            this.logger.LogInformation("Published Integration event {Event} {Id}", @event.GetType().Name, @event.Id);
        }

        public async Task SendCommandAsync<T>(T command, Uri endpointAddress, CancellationToken cancellationToken = default)
            where T : class, IIntegrationCommand
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            var endpoint = await this.sendEndpointProvider.GetSendEndpoint(endpointAddress);

            command.DatePublishedOnUtc = this.dateTime.UtcNow;

            await endpoint.Send(command, cancellationToken);

            this.logger.LogInformation("Sent Integration command: {Command} {Id}", command.GetType().Name, command.Id);
        }
    }
}