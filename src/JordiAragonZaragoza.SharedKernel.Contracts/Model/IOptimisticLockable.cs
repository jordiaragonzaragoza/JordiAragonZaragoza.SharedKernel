namespace JordiAragonZaragoza.SharedKernel.Contracts.Model
{
    public interface IOptimisticLockable
    {
        uint? Version { get; }
    }
}