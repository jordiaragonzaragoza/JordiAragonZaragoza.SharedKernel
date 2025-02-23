namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Ardalis.Result;
    using global::EventStore.Client;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Serialization;
    using Microsoft.Extensions.Logging;

    public class EventStoreDbEventStore : IEventStore, IUnitOfWork, IScopedDependency
    {
        private readonly List<IEventSourcedAggregateRoot<IEntityId>> pendingChanges = [];
        private readonly EventStoreClient eventStoreClient;
        private readonly ILogger<EventStoreDbEventStore> logger;

        public EventStoreDbEventStore(
            EventStoreClient eventStoreClient,
            ILogger<EventStoreDbEventStore> logger)
        {
            this.eventStoreClient = Guard.Against.Null(eventStoreClient, nameof(eventStoreClient));
            this.logger = Guard.Against.Null(logger, nameof(logger));
        }

        public IEnumerable<IEventsContainer<IEvent>> EventableEntities
            => this.pendingChanges.AsReadOnly();

        public async Task<TAggregate?> LoadAggregateAsync<TAggregate, TId>(TId aggregateId, CancellationToken cancellationToken = default)
            where TAggregate : class, IEventSourcedAggregateRoot<TId>
            where TId : class, IEntityId
        {
            ArgumentNullException.ThrowIfNull(aggregateId, nameof(aggregateId));

            var readResult = this.eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                StreamNameMapper.ToStreamId<TAggregate>(aggregateId),
                StreamPosition.Start,
                cancellationToken: cancellationToken);

            if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            {
                return null;
            }

            // If this reflection causes performance issues, use a public constructors on aggregates if its required.
            var aggregate = Activator.CreateInstance(typeof(TAggregate), true) as TAggregate;

            var domainEvents = new List<IDomainEvent>();
            await foreach (var resolvedEvent in readResult)
            {
                var domainEvent = SerializerHelper.Deserialize(resolvedEvent);
                domainEvents.Add(domainEvent);
            }

            if (aggregate != null)
            {
                this.logger.LogInformation("Loading events for the aggregate: {Aggregate}", aggregate.ToString());

                aggregate.Load(domainEvents);
            }

            return aggregate;
        }

        public void AppendChanges<TAggregate, TId>(TAggregate aggregate)
            where TAggregate : class, IEventSourcedAggregateRoot<TId>
            where TId : class, IEntityId
        {
            this.pendingChanges.Add(aggregate);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var aggregateToSave in this.pendingChanges)
            {
                await this.StoreAsync(aggregateToSave, cancellationToken);
            }

            this.pendingChanges.Clear();
        }

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> operation)
            where TResponse : IResult
        {
            ArgumentNullException.ThrowIfNull(operation, nameof(operation));

            var response = await operation();

            // Get Ardalis.Result.IsSuccess or Ardalis.Result<T>.IsSuccess
            var isSuccessResponse = typeof(TResponse).GetProperty("IsSuccess")?.GetValue(response, null) ?? false;
            if ((bool)isSuccessResponse)
            {
                await this.SaveChangesAsync();
            }

            return response;
        }

        private async Task StoreAsync(IEventSourcedAggregateRoot<IEntityId> aggregate, CancellationToken cancellationToken)
        {
            var events = aggregate.Events.AsEnumerable().Select(static @event => SerializerHelper.Serialize(@event)).ToArray();

            if (events.Length == 0)
            {
                return;
            }

            var streamName = StreamNameMapper.ToStreamId(aggregate.GetType(), aggregate.Id);

            long version = aggregate.Version == default ? -1 : aggregate.Version;
            var nextVersion = StreamRevision.FromInt64(version);

            foreach (var @event in events)
            {
                this.logger.LogInformation("Persisting event: {Event} for stream: {StreamName}", @event.ToString(), streamName);
            }

            _ = await this.eventStoreClient.AppendToStreamAsync(
                streamName,
                nextVersion,
                events,
                cancellationToken: cancellationToken);

            aggregate.ClearEvents();
        }
    }
}