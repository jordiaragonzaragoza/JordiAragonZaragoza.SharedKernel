namespace JordiAragonZaragoza.SharedKernel.Domain.Events
{
    using System;
    using System.Text.Json.Serialization;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public abstract record class BaseDomainEvent(Guid AggregateId) : IDomainEvent
    {
        public Guid Id { get; protected init; } = Guid.CreateVersion7();

        [JsonIgnore]
        public DateTimeOffset DateOccurredOnUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}