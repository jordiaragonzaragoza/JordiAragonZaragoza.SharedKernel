namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Metadata
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public sealed class ReadModelEnvelope<TReadModel> : IPartitionable
        where TReadModel : class, IReadModel
    {
        public TReadModel Data { get; set; } = default!;

        public string TenantId { get; set; } = string.Empty;

        public string PartitionClientId { get; set; } = string.Empty;
    }
}