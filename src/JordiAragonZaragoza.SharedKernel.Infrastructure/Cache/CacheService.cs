namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Cache
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    // TODO: Complete implementation using Microsoft.Extensions.Caching.Hybrid
    public class CacheService : ICacheService
    {
        public Task<ICacheValue<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}