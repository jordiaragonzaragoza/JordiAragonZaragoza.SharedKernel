namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition
{
    using System;
    using System.Threading;

    public sealed class PartitionContextService : IPartitionContextService
    {
        private static readonly AsyncLocal<PartitionContext?> AsyncPartitionContext = new();

        public PartitionContext CurrentContext =>
            AsyncPartitionContext.Value ?? throw new InvalidOperationException("PartitionContext has not been set.");

        public void SetPartitionContext(string tenantId, string partitionId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("TenantId cannot be null or empty.", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(partitionId))
            {
                throw new ArgumentException("PartitionId cannot be null or empty.", nameof(partitionId));
            }

            AsyncPartitionContext.Value = new PartitionContext(tenantId, partitionId);
        }
    }
}