namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition;
    using global::MassTransit;

    public sealed class PartitionContextProducerFilter<T>
        : IFilter<PublishContext<T>>, IFilter<SendContext<T>>
        where T : class
    {
        private readonly IPartitionContextService partitionContextService;

        public PartitionContextProducerFilter(
            IPartitionContextService partitionContextService)
        {
            this.partitionContextService = partitionContextService ?? throw new ArgumentNullException(nameof(partitionContextService));
        }

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            this.SetHeaders(context);

            return next.Send(context);
        }

        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            this.SetHeaders(context);

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("PartitionContextProducerFilter");

        private void SetHeaders(SendContext context)
        {
            try
            {
                var partitionContext = this.partitionContextService.CurrentContext;

                context.Headers.Set(PartitionConstants.TenantId, partitionContext.TenantId);
                context.Headers.Set(PartitionConstants.PartitionId, partitionContext.PartitionId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Partition context must be set before publishing or sending a message.", ex);
            }
        }
    }
}