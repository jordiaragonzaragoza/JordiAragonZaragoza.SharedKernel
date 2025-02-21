namespace JordiAragonZaragoza.SharedKernel.Domain.Events.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Autofac;
    using Autofac.Core;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Contracts.Outbox;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public class EventsDispatcherService : IEventsDispatcherService, IScopedDependency
    {
        private readonly IEventBus eventBus;
        private readonly ILifetimeScope scope;
        private readonly IOutboxService outboxService;
        private readonly IAggregatesStore aggregatesStore;

        public EventsDispatcherService(
            IAggregatesStore aggregatesStore,
            IEventBus eventBus,
            ILifetimeScope scope,
            IOutboxService outboxService)
        {
            this.aggregatesStore = Guard.Against.Null(aggregatesStore, nameof(aggregatesStore));
            this.eventBus = Guard.Against.Null(eventBus, nameof(eventBus));
            this.scope = Guard.Against.Null(scope, nameof(scope));
            this.outboxService = Guard.Against.Null(outboxService, nameof(outboxService));
        }

        public async Task DispatchEventsFromEventableEntitiesAsync(IEnumerable<IEventsContainer<IEvent>> eventableEntities, CancellationToken cancellationToken = default)
        {
            var eventables = eventableEntities.ToList();
            if (eventables.Count == 0)
            {
                return;
            }

            await this.DispatchEventsAsync(eventables, cancellationToken);
        }

        public async Task DispatchEventsFromAggregatesStoreAsync(CancellationToken cancellationToken = default)
        {
            var eventables = this.aggregatesStore.EventableEntities.ToList();
            if (eventables.Count == 0)
            {
                return;
            }

            await this.DispatchEventsAsync(eventables, cancellationToken);
        }

        private async Task DispatchEventsAsync(List<IEventsContainer<IEvent>> eventables, CancellationToken cancellationToken)
        {
            var events = eventables.SelectMany(x => x.Events).Where(e => !e.IsPublished).OrderBy(e => e.DateOccurredOnUtc).ToList();

            // Filter to not include IEventSourcedAggregateRoot events.
            // This event notifications will come from event store subscription.
            var aggregateEvents = eventables.Where(entity => entity is not IEventSourcedAggregateRoot<IEntityId>)
                .SelectMany(x => x.Events).Where(e => !e.IsPublished).OrderBy(e => e.DateOccurredOnUtc).ToList();

            var eventNotifications = this.CreateEventNotifications(aggregateEvents);

            await this.PublishEventsAsync(events, cancellationToken);

            await this.StoreEventNotificationsAsync(eventNotifications, cancellationToken);
        }

        private List<IEventNotification<IEvent>> CreateEventNotifications(IEnumerable<IEvent> events)
        {
            var eventNotifications = new List<IEventNotification<IEvent>>();
            foreach (var @event in events)
            {
                // Instanciate the event notification.
                var eventNotificationType = typeof(IEventNotification<>);
                var notificationWithGenericType = eventNotificationType.MakeGenericType(@event.GetType());
                var notification = this.scope.ResolveOptional(notificationWithGenericType, new List<Parameter>
                {
                    new NamedParameter("Event", @event),
                });

                if (notification != null)
                {
                    eventNotifications.Add((IEventNotification<IEvent>)notification);
                }
            }

            return eventNotifications;
        }

        private async Task PublishEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                await this.eventBus.PublishAsync(@event, cancellationToken);
            }
        }

        private async Task StoreEventNotificationsAsync(IEnumerable<IEventNotification<IEvent>> eventNotifications, CancellationToken cancellationToken)
        {
            foreach (var eventNotification in eventNotifications)
            {
                await this.outboxService.AddMessageAsync(eventNotification, cancellationToken);
            }
        }
    }
}