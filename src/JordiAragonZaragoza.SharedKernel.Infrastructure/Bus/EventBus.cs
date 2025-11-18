namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using global::MassTransit;
    using Microsoft.Extensions.Logging;

    public class EventBus : IEventBus
    {
        private readonly IPublishEndpoint publishEndPoint;
        private readonly ILogger<EventBus> logger;
        private readonly IDateTime dateTime;

        public EventBus(
            IPublishEndpoint publishEndPoint,
            ILogger<EventBus> logger,
            IDateTime dateTime)
        {
            this.publishEndPoint = publishEndPoint ?? throw new ArgumentNullException(nameof(publishEndPoint));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            try
            {
                await this.publishEndPoint.Publish(@event, @event.GetType(), cancellationToken);

                this.logger.LogDebug("Published event {Event} {Id} at {DateTime}", @event.GetType().Name, @event.Id, this.dateTime.UtcNow);
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                   exception,
                   "Error publishing event: {@Name} {Id} {Content} at {DateTime}",
                   @event.GetType().Name,
                   @event.Id,
                   @event,
                   this.dateTime.UtcNow);

                throw;
            }
        }
    }
}