namespace JordiAragonZaragoza.SharedKernel.Domain.Events
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    /// <summary>
    /// Use this base for domain events that must travel through the in-memory
    /// bus dispatched by EventsDispatcherService (EF Core aggregates).
    /// EventSourced aggregates use BaseDomainEvent directly — their events
    /// are dispatched via KurrentDB subscription, not in-memory.
    /// </summary>
    public abstract record class BaseInMemoryDomainEvent(Guid AggregateId) : BaseDomainEvent(AggregateId), IInMemoryEvent
    {
        public bool IsPublished { get; set; }
    }
}