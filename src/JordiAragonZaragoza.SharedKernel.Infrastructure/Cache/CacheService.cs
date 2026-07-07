namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Cache
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Caching.Hybrid;

    /// <summary>
    /// Cache service implementation using Microsoft.Extensions.Caching.Hybrid.
    /// HybridCache provides a multi-layer caching strategy combining in-memory and distributed caching.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly HybridCache cache;
        private readonly ConcurrentDictionary<string, HashSet<string>> prefixKeyRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="cache">The hybrid cache instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when cache is null.</exception>
        public CacheService(HybridCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.prefixKeyRegistry = new ConcurrentDictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A cache value wrapper indicating whether the value exists and its content.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when cacheKey is null or whitespace.</exception>
        public async Task<ICacheValue<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);

            try
            {
                // Use wrapper to distinguish between cached values and cache misses
                var wrapper = await this.cache.GetOrCreateAsync<object?>(
                    cacheKey,
                    async _ =>
                    {
                        // Factory is only called if key is not in cache - indicates a cache miss
                        return new CachedValueWrapper { Value = default, IsFromFactory = true };
                    },
                    cancellationToken: cancellationToken);

                var foundInCache = wrapper is CachedValueWrapper cw && !cw.IsFromFactory;
                var cachedValue = foundInCache && wrapper is CachedValueWrapper w ? (T?)w.Value : default;

                return new HybridCacheValue<T>(cachedValue, foundInCache);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // Cache operation failed, return empty value
                System.Diagnostics.Debug.WriteLine($"Cache get failed: {ex.Message}");
                return new HybridCacheValue<T>(default, false);
            }
        }

        /// <summary>
        /// Sets a value in the cache asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheValue">The value to cache.</param>
        /// <param name="expiration">The expiration time span. If null, no expiration is set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when cacheKey is null or whitespace.</exception>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);

            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration,
                LocalCacheExpiration = expiration,
            };

            await this.cache.SetAsync(cacheKey, cacheValue, options, cancellationToken: cancellationToken);

            // Track the key by its prefix for RemoveByPrefixAsync
            var prefix = ExtractPrefix(cacheKey);
            if (!string.IsNullOrEmpty(prefix))
            {
                this.prefixKeyRegistry.AddOrUpdate(
                    prefix,
                    _ => new HashSet<string> { cacheKey },
                    (_, keys) =>
                    {
                        lock (keys)
                        {
                            keys.Add(cacheKey);
                        }

                        return keys;
                    });
            }
        }

        /// <summary>
        /// Removes all cached entries that start with the specified prefix.
        /// </summary>
        /// <remarks>
        /// This implementation tracks keys internally by prefix and removes them individually.
        /// Note: Keys set outside of this service or through distributed cache mechanisms may not be tracked.
        /// </remarks>
        /// <param name="prefix">The prefix of cache keys to remove.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when prefix is null or whitespace.</exception>
        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

            // Retrieve all keys with this prefix from the registry
            if (this.prefixKeyRegistry.TryRemove(prefix, out var keys))
            {
                var removeTasks = keys.Select(key => this.cache.RemoveAsync(key, cancellationToken).AsTask());
                await Task.WhenAll(removeTasks);
            }
        }

        /// <summary>
        /// Extracts the prefix from a cache key (everything before the first underscore).
        /// Cache key format is: "{TypeFullName}_{Identifier}" where TypeFullName uses '.' as separator
        /// and should not contain '_'. The identifier may contain '_' (e.g., GUIDs), so we extract
        /// everything before the FIRST underscore.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>
        /// The prefix of the cache key, or the full key if no underscore is found.
        /// </returns>
        private static string ExtractPrefix(string cacheKey)
        {
            var firstUnderscoreIndex = cacheKey.IndexOf('_', StringComparison.Ordinal);
            return firstUnderscoreIndex > 0 ? cacheKey.Substring(0, firstUnderscoreIndex) : cacheKey;
        }

        /// <summary>
        /// Wrapper class to distinguish between cached values and cache misses.
        /// </summary>
        private sealed class CachedValueWrapper
        {
            /// <summary>
            /// Gets or sets the cached value.
            /// </summary>
            public object? Value { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this value was created by the factory (cache miss).
            /// </summary>
            public bool IsFromFactory { get; set; }
        }
    }
}