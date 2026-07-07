namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    /// <summary>
    /// ⚠️ Warning:. DO NOT USE WHEN USING EVENT SOURCING, AS IT CAN CAUSE INCONSISTENCIES.
    /// This interface is intended to be implemented by requests that require caching when processed, but not when using projections in an event bus pipeline.
    /// </summary>
    public interface ICacheRequest
    {
        string CacheKey { get; } // TODO: Review GetType().FullName or Aggregate

        TimeSpan? AbsoluteExpirationInSeconds { get; }
    }
}