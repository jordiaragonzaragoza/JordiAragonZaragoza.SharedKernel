namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Producers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using global::MassTransit;
    using global::MassTransit.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MassTransitIntegrationEventBus : IIntegrationEventBus
    {
        private readonly Bind<IIntegrationBus, IPublishEndpoint> publishEndPoint;
        private readonly ILogger<MassTransitIntegrationEventBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationEventBus(
            Bind<IIntegrationBus, IPublishEndpoint> publishEndPoint,
            ILogger<MassTransitIntegrationEventBus> logger,
            IDateTime dateTime)
        {
            this.publishEndPoint = publishEndPoint ?? throw new ArgumentNullException(nameof(publishEndPoint));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IIntegrationEvent
        {
            ArgumentNullException.ThrowIfNull(@event);

            try
            {
                @event.DateDispatchedOnUtc = this.dateTime.UtcNow;

                await this.publishEndPoint.Value.Publish(@event, cancellationToken);

                this.logger.LogDebug("Published integration event {Event} {Id} at {DateTime}", @event.GetType().Name, @event.Id, this.dateTime.UtcNow);
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                   exception,
                   "Error publishing integration event: {@Name} {Id} {Content} at {DateTime}",
                   @event.GetType().Name,
                   @event.Id,
                   @event,
                   this.dateTime.UtcNow);

                throw;
            }
        }
    }
}