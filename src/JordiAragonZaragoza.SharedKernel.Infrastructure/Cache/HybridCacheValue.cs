namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Cache
{
    using System.Collections.Generic;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    /// <summary>
    /// Implementation of ICacheValue for HybridCache.
    /// Wraps the retrieved value and provides information about its existence.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    internal sealed class HybridCacheValue<T> : ICacheValue<T>
    {
        private readonly T? value;
        private readonly bool cacheFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridCacheValue{T}"/> class.
        /// </summary>
        /// <param name="value">The cached value.</param>
        /// <param name="cacheFound">Indicates whether the value was found in the cache.</param>
        public HybridCacheValue(T? value, bool cacheFound = true)
        {
            this.value = value;
            this.cacheFound = cacheFound;
        }

        /// <summary>
        /// Gets a value indicating whether a value exists in the cache.
        /// </summary>
        /// <remarks>
        /// Returns true only if the value was found in the cache.
        /// This correctly distinguishes between "not in cache" and "cached null/default value".
        /// </remarks>
        /// <returns>
        /// <c>true</c> if a value exists in the cache; otherwise, <c>false</c>.
        /// </returns>
        public bool HasValue => this.cacheFound;

        /// <summary>
        /// Gets a value indicating whether the value is null or not found.
        /// </summary>
        /// <remarks>
        /// Returns true if either:
        /// - The value was not found in the cache, OR
        /// - The cached value itself is null/default.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if the value is null, default, or not found; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNull => !this.cacheFound || EqualityComparer<T>.Default.Equals(this.value, default);

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// <returns>
        /// The cached value.
        /// </returns>
#pragma warning disable CS8766
        public T? Value => this.value;
#pragma warning restore CS8766
    }
}