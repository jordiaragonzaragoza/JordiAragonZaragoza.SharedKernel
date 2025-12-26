namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using global::MediatR;
    using Microsoft.Extensions.Logging;

    public class InMemoryEventBus : IEventBus
    {
        private readonly IPublisher publisher;
        private readonly ILogger<InMemoryEventBus> logger;
        private readonly IDateTime dateTime;

        public InMemoryEventBus(
            IPublisher publisher,
            ILogger<InMemoryEventBus> logger,
            IDateTime dateTime)
        {
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            try
            {
                @event.IsPublished = true;

                this.logger.LogDebug("Published in memory event {Event} {Id} at {DateTime}", @event.GetType().Name, @event.Id, this.dateTime.UtcNow);

                await this.publisher.Publish(@event, cancellationToken).ConfigureAwait(true);
            }
            catch (Exception exception)
            {
                @event.IsPublished = false;

                this.logger.LogError(
                   exception,
                   "Error publishing in memory event: {@Name} {Id} {Content} at {DateTime}",
                   @event.GetType().Name,
                   @event.Id,
                   @event,
                   this.dateTime.UtcNow);

                throw;
            }
        }
    }
}