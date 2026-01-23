namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Metadata
{
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public sealed class AggregateEnvelope<TAggregate, TId> : ISoftDeletable, IPartitionable
        where TAggregate : IAggregateRoot<TId>
        where TId : class, IEntityId
    {
        public TAggregate Data { get; set; } = default!;

        public bool IsDelete { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string PartitionClientId { get; set; } = string.Empty;
    }
}