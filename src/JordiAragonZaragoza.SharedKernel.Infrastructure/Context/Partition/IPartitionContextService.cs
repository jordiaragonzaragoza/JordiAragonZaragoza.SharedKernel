namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition
{
    public interface IPartitionContextService
    {
        PartitionContext CurrentContext { get; }

        void SetPartitionContext(string tenantId, string partitionId);
    }
}