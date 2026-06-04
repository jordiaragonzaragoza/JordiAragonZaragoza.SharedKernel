namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using global::KurrentDB.Client;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization;
    using Microsoft.Extensions.Logging;

    public class KurrentDbEventStore : IEventStore, IUnitOfWork
    {
        private readonly List<IEventSourcedAggregateRoot<IEntityId>> pendingChanges = [];
        private readonly KurrentDBClient eventStoreClient;
        private readonly IExecutionContextService executionContextService;

        private readonly ILogger<KurrentDbEventStore> logger;

        public KurrentDbEventStore(
            KurrentDBClient eventStoreClient,
            IExecutionContextService executionContextService,
            ILogger<KurrentDbEventStore> logger)
        {
            this.eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.executionContextService = executionContextService ?? throw new ArgumentNullException(nameof(executionContextService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                var (domainEvent, _) = SerializerHelper.Deserialize(resolvedEvent);
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

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> operation, CancellationToken cancellationToken = default)
            where TResponse : IResult
        {
            ArgumentNullException.ThrowIfNull(operation, nameof(operation));

            var response = await operation();

            if (response.Status is ResultStatus.Ok or ResultStatus.NoContent or ResultStatus.Created)
            {
                await this.SaveChangesAsync(cancellationToken);
            }

            return response;
        }

        private async Task StoreAsync(IEventSourcedAggregateRoot<IEntityId> aggregate, CancellationToken cancellationToken)
        {
            var executionContext = this.executionContextService.CurrentContext;
            var events = aggregate.Events.AsEnumerable().Select(@event => SerializerHelper.Serialize(@event, executionContext)).ToArray();

            if (events.Length == 0)
            {
                return;
            }

            // TODO: Add TenantId.
            ////var streamName = StreamNameMapper.ToStreamId(aggregate.GetType(), aggregate.Id, executionContext?.ScopeContext.TenantId);
            var streamName = StreamNameMapper.ToStreamId(aggregate.GetType(), aggregate.Id);

            var expectedState = aggregate.Version is null
                ? StreamState.NoStream
                : StreamState.StreamRevision((ulong)aggregate.Version);

            foreach (var @event in events)
            {
                this.logger.LogInformation("Persisting event: {Event} for stream: {StreamName}", @event.ToString(), streamName);
            }

            _ = await this.eventStoreClient.AppendToStreamAsync(
                streamName,
                expectedState,
                events,
                cancellationToken: cancellationToken);

            aggregate.ClearEvents();
        }
    }
}