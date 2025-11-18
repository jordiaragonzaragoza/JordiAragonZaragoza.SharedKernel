namespace JordiAragonZaragoza.SharedKernel.Domain.Events.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public class EventsDispatcherService : IEventsDispatcherService
    {
        private readonly IInMemoryEventBus inMemoryEventBus;
        private readonly IEventBus eventBus;
        private readonly IAggregateStore aggregatesStore;

        public EventsDispatcherService(
            IAggregateStore aggregatesStore,
            IInMemoryEventBus inMemoryEventBus,
            IEventBus eventBus)
        {
            this.aggregatesStore = aggregatesStore ?? throw new ArgumentNullException(nameof(aggregatesStore));
            this.inMemoryEventBus = inMemoryEventBus ?? throw new ArgumentNullException(nameof(inMemoryEventBus));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task DispatchEventsFromAggregatesStoreAsync(CancellationToken cancellationToken = default)
        {
            var eventables = this.aggregatesStore.EventableEntities.ToList();
            if (eventables.Count == 0)
            {
                return;
            }

            var events = eventables.SelectMany(static x => x.Events).Where(static e => !e.IsPublished).OrderBy(static e => e.DateOccurredOnUtc).ToList();

            // Filter to not include IEventSourcedAggregateRoot events.
            // This event notifications will come from event store subscription.
            var aggregateEvents = eventables.Where(static entity => entity is not IEventSourcedAggregateRoot<IEntityId>)
                .SelectMany(static x => x.Events).Where(static e => !e.IsPublished).OrderBy(static e => e.DateOccurredOnUtc).ToList();

            await this.PublishInMemoryEventsAsync(events, cancellationToken);

            await this.PublishEventsAsync(aggregateEvents, cancellationToken);
        }

        private async Task PublishInMemoryEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                await this.inMemoryEventBus.PublishAsync(@event, cancellationToken);
            }
        }

        private async Task PublishEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                await this.eventBus.PublishAsync(@event, cancellationToken);
            }
        }
    }
}