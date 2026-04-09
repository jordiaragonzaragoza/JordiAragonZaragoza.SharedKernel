namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public sealed record class ScopeContext(
        Guid TenantId,
        Guid? PartitionId,
        Guid? DomainId);
}