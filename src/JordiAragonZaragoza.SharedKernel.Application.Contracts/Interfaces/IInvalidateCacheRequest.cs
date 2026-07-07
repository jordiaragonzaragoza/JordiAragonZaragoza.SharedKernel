namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This interface is intended to be implemented by requests that require cache invalidation when processed, but not when using projections in an event bus pipeline.
    /// </summary>
    public interface IInvalidateCacheRequest
    {
        public string PrefixCacheKey { get; }
    }
}