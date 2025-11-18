namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition
{
    public sealed record class PartitionContext(string TenantId, string PartitionId);
}