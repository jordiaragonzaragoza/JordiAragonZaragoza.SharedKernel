namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public sealed record class ScopeContext
    {
        public ScopeContext(
            Guid tenantId,
            Guid? partitionId,
            Guid? domainId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("TenantId is required", nameof(tenantId));
            }

            // Reject Guid.Empty explicitly for optional fields so that callers
            // cannot accidentally store a meaningless all-zeros value that
            // would pass the null check but carry no real information.
            if (partitionId.HasValue && partitionId.Value == Guid.Empty)
            {
                throw new ArgumentException("PartitionId must not be empty when provided.", nameof(partitionId));
            }

            if (domainId.HasValue && domainId.Value == Guid.Empty)
            {
                throw new ArgumentException("DomainId must not be empty when provided.", nameof(domainId));
            }

            this.TenantId = tenantId;
            this.PartitionId = partitionId;
            this.DomainId = domainId;
        }

        public Guid TenantId { get; }

        public Guid? PartitionId { get; }

        public Guid? DomainId { get; }
    }
}