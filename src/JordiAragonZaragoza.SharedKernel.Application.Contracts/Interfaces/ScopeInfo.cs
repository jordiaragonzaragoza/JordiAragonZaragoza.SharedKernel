namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    /// <summary>
    /// Flat scope representation for read models. Uses primitive Guids
    /// (not domain value objects) to stay decoupled from domain types.
    /// </summary>
    public sealed class ScopeInfo
    {
        public ScopeInfo(Guid tenantId, Guid? partitionId = null, Guid? domainId = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("TenantId is required.", nameof(tenantId));
            }

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

        private ScopeInfo()
        {
        }

        public Guid TenantId { get; private set; }

        public Guid? PartitionId { get; private set; }

        public Guid? DomainId { get; private set; }

        /// <summary>
        /// Creates a ScopeInfo from the application-layer ScopeContext,
        /// mapping DomainId to the same concept without domain type dependency.
        /// </summary>
        /// <param name="scopeContext">The scope context to convert.</param>
        /// <returns>A new ScopeInfo instance.</returns>
        public static ScopeInfo From(ScopeContext scopeContext)
        {
            ArgumentNullException.ThrowIfNull(scopeContext);

            return new ScopeInfo(scopeContext.TenantId, scopeContext.PartitionId, scopeContext.DomainId);
        }

        /// <summary>
        /// Mirrors User.Domain.Scope.Matches semantics: the narrowest defined
        /// level wins. A read model with no PartitionId/DomainId is visible
        /// across the entire tenant.
        /// </summary>
        /// <param name="tenantId">The tenant to match.</param>
        /// <param name="partitionId">The partition to match.</param>
        /// <param name="domainId">The domain to match.</param>
        /// <returns>True if the scope matches, false otherwise.</returns>
        public bool Matches(Guid tenantId, Guid? partitionId, Guid? domainId)
        {
            if (this.DomainId is not null)
            {
                return this.DomainId == domainId;
            }

            if (this.PartitionId is not null)
            {
                return this.PartitionId == partitionId;
            }

            return this.TenantId == tenantId;
        }
    }
}