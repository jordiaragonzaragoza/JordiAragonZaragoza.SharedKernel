namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Metadata
{
    public interface IPartitionable
    {
        string TenantId { get; set; }

        string PartitionClientId { get; set; }
    }
}