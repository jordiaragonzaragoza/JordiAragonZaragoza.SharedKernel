namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition;
    using global::MassTransit;

    public sealed class PartitionContextConsumerFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly IPartitionContextService partitionContextService;

        public PartitionContextConsumerFilter(
            IPartitionContextService partitionContextService)
        {
            this.partitionContextService = partitionContextService ?? throw new ArgumentNullException(nameof(partitionContextService));
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            var tenantId = context.Headers.Get<string>(PartitionConstants.TenantId);
            var partitionId = context.Headers.Get<string>(PartitionConstants.PartitionId);

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(partitionId))
            {
                if (AnonymousMessageHelper.IsAnonymousAllowed(context))
                {
                    tenantId = PartitionConstants.AnonymousTenant;
                    partitionId = PartitionConstants.AnonymousPartition;
                }
                else
                {
                    throw new InvalidOperationException("Missing TenantId or PartitionId headers in message.");
                }
            }

            this.partitionContextService.SetPartitionContext(tenantId!, partitionId!);

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("PartitionContextConsumerFilter");
    }
}