namespace JordiAragonZaragoza.SharedKernel.Domain.Events
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public abstract record class BaseDomainEvent(Guid AggregateId) : IDomainEvent
    {
        public Guid Id { get; protected init; } = Guid.NewGuid();

        public bool IsPublished { get; set; } // TODO: Add to Event Metadata

        public DateTimeOffset DateOccurredOnUtc { get; protected init; } = DateTimeOffset.UtcNow; // TODO: Add to Event Metadata
    }
}