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
                throw new ArgumentException("TenantId is required");
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