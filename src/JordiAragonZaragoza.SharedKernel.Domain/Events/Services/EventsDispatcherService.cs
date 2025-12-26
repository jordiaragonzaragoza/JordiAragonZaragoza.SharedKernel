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
        private readonly IEventBus inMemoryEventBus;
        private readonly IAggregateStore aggregatesStore;

        public EventsDispatcherService(
            IAggregateStore aggregatesStore,
            IEventBus inMemoryEventBus)
        {
            this.aggregatesStore = aggregatesStore ?? throw new ArgumentNullException(nameof(aggregatesStore));
            this.inMemoryEventBus = inMemoryEventBus ?? throw new ArgumentNullException(nameof(inMemoryEventBus));
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
            /*var aggregateEvents = eventables.Where(static entity => entity is not IEventSourcedAggregateRoot<IEntityId>)
                .SelectMany(static x => x.Events).Where(static e => !e.IsPublished).OrderBy(static e => e.DateOccurredOnUtc).ToList();*/

            await this.PublishInMemoryEventsAsync(events, cancellationToken);
        }

        private async Task PublishInMemoryEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                await this.inMemoryEventBus.PublishAsync(@event, cancellationToken);
            }
        }
    }
}