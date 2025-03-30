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

    public class MassTransitIntegrationEventBus : IIntegrationEventBus, IScopedDependency
    {
        private readonly IPublishEndpoint publishEndPoint;
        private readonly ILogger<MassTransitIntegrationEventBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationEventBus(
            IPublishEndpoint publishEndPoint,
            ILogger<MassTransitIntegrationEventBus> logger,
            IDateTime dateTime)
        {
            this.publishEndPoint = publishEndPoint ?? throw new ArgumentNullException(nameof(publishEndPoint));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IIntegrationEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            @event.DateDispatchedOnUtc = this.dateTime.UtcNow;

            await this.publishEndPoint.Publish(@event, cancellationToken);

            this.logger.LogInformation("Published Integration event {Event} {Id}", @event.GetType().Name, @event.Id);
        }
    }
}