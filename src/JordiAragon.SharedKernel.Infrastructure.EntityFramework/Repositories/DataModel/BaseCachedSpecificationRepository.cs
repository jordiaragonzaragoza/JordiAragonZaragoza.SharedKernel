﻿namespace JordiAragon.SharedKernel.Infrastructure.EntityFramework.Repositories.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Ardalis.Specification;
    using JordiAragon.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Contracts.DependencyInjection;
    using JordiAragon.SharedKernel.Contracts.Repositories;
    using JordiAragon.SharedKernel.Infrastructure.EntityFramework.Context;
    using JordiAragon.SharedKernel.Infrastructure.Interfaces;
    using Microsoft.Extensions.Logging;

    public abstract class BaseCachedSpecificationRepository<TDataEntity> : BaseReadRepository<TDataEntity>, ICachedSpecificationRepository<TDataEntity, Guid>, IScopedDependency
        where TDataEntity : class, IDataEntity
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<BaseCachedSpecificationRepository<TDataEntity>> logger;

        protected BaseCachedSpecificationRepository(
            BaseBusinessModelContext dbContext,
            ILogger<BaseCachedSpecificationRepository<TDataEntity>> logger,
            ICacheService cacheService)
            : base(dbContext)
        {
            this.cacheService = Guard.Against.Null(cacheService, nameof(cacheService));
            this.logger = Guard.Against.Null(logger, nameof(logger));
        }

        public string CacheKey => $"{typeof(TDataEntity)}";

        public override async Task<TDataEntity> AddAsync(TDataEntity entity, CancellationToken cancellationToken = default)
        {
            var response = await base.AddAsync(entity, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
            return response;
        }

        public override async Task<IEnumerable<TDataEntity>> AddRangeAsync(IEnumerable<TDataEntity> entities, CancellationToken cancellationToken = default)
        {
            var response = await base.AddRangeAsync(entities, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
            return response;
        }

        public override async Task UpdateAsync(TDataEntity entity, CancellationToken cancellationToken = default)
        {
            await base.UpdateAsync(entity, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
        }

        public override async Task UpdateRangeAsync(IEnumerable<TDataEntity> entities, CancellationToken cancellationToken = default)
        {
            await base.UpdateRangeAsync(entities, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
        }

        public override async Task DeleteAsync(TDataEntity entity, CancellationToken cancellationToken = default)
        {
            await base.DeleteAsync(entity, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
        }

        public override async Task DeleteRangeAsync(IEnumerable<TDataEntity> entities, CancellationToken cancellationToken = default)
        {
            await base.DeleteRangeAsync(entities, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
        }

        public override async Task<TDataEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKeyId = $"{this.CacheKey}_{id}";

            var cacheResponse = await this.CacheGetAsync<TDataEntity>(cacheKeyId, cancellationToken);
            if (cacheResponse != null)
            {
                return cacheResponse;
            }

            var response = await base.GetByIdAsync(id, cancellationToken);

            await this.CacheSetAsync(cacheKeyId, response, cancellationToken);
            return response;
        }

        public override async Task<TDataEntity?> FirstOrDefaultAsync(ISpecification<TDataEntity> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";

            var cachedResponse = await this.CacheGetAsync<TDataEntity>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.FirstOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TDataEntity, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.cacheService.GetAsync<TResult>(cacheKeySpecification, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKeySpecification}", cacheKeySpecification);

                return cachedResponse.Value;
            }

            var response = await base.FirstOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<TDataEntity?> SingleOrDefaultAsync(ISingleResultSpecification<TDataEntity> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.CacheGetAsync<TDataEntity>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.SingleOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TDataEntity, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";

            var cachedResponse = await this.cacheService.GetAsync<TResult>(cacheKeySpecification, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKeySpecification}", cacheKeySpecification);

                return cachedResponse.Value;
            }

            var response = await base.SingleOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<List<TDataEntity>> ListAsync(CancellationToken cancellationToken = default)
        {
            var cachedResponse = await this.CacheGetListAsync<TDataEntity>(this.CacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.ListAsync(cancellationToken);

            await this.CacheSetListAsync(this.CacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<List<TDataEntity>> ListAsync(ISpecification<TDataEntity> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.CacheGetListAsync<TDataEntity>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.ListAsync(specification, cancellationToken);

            await this.CacheSetListAsync(cacheKeySpecification, response, cancellationToken);
            return response;
        }

        public override async Task<List<TResult>> ListAsync<TResult>(ISpecification<TDataEntity, TResult> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.CacheGetListAsync<TResult>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.ListAsync(specification, cancellationToken);

            await this.CacheSetListAsync(cacheKeySpecification, response, cancellationToken);
            return response;
        }

        public override async Task<int> CountAsync(ISpecification<TDataEntity> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";

            var cachedResponse = await this.cacheService.GetAsync<int>(cacheKeySpecification, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKeySpecification}", cacheKeySpecification);

                return cachedResponse.Value;
            }

            var response = await base.CountAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var cachedResponse = await this.cacheService.GetAsync<int>(this.CacheKey, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", this.CacheKey);

                return cachedResponse.Value;
            }

            var response = await base.CountAsync(cancellationToken);

            await this.CacheSetAsync(this.CacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<bool> AnyAsync(ISpecification<TDataEntity> specification, CancellationToken cancellationToken = default)
        {
            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.cacheService.GetAsync<bool>(cacheKeySpecification, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKeySpecification}", cacheKeySpecification);

                return cachedResponse.Value;
            }

            var response = await base.AnyAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            var cachedResponse = await this.cacheService.GetAsync<bool>(this.CacheKey, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", this.CacheKey);

                return cachedResponse.Value;
            }

            var response = await base.AnyAsync(cancellationToken);

            await this.CacheSetAsync(this.CacheKey, response, cancellationToken);

            return response;
        }

        private async Task<T?> CacheGetAsync<T>(string cacheKey, CancellationToken cancellationToken)
            where T : class
        {
            var cachedResponse = await this.cacheService.GetAsync<T>(cacheKey, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);

                return cachedResponse.Value;
            }

            return default;
        }

        private async Task<List<TIn>?> CacheGetListAsync<TIn>(string cacheKey, CancellationToken cancellationToken)
        {
            var cachedResponse = await this.cacheService.GetAsync<List<TIn>>(cacheKey, cancellationToken);
            if (!cachedResponse.IsNull && cachedResponse.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);

                return cachedResponse.Value;
            }

            return default;
        }

        private async Task CacheSetAsync<TIn>(string cacheKey, TIn? response, CancellationToken cancellationToken)
        {
            await this.cacheService.SetAsync(cacheKey, response, cancellationToken: cancellationToken);

            this.logger.LogInformation("Set data to cache with  cacheKey: {CacheKey}", cacheKey);
        }

        private async Task CacheSetListAsync<TIn>(string cacheKey, List<TIn> response, CancellationToken cancellationToken)
        {
            await this.cacheService.SetAsync(cacheKey, response, cancellationToken: cancellationToken);

            this.logger.LogInformation("Set data to cache with  cacheKey: {CacheKey}", cacheKey);
        }

        private async Task CacheServiceRemoveByPrefixAsync(CancellationToken cancellationToken)
        {
            await this.cacheService.RemoveByPrefixAsync(this.CacheKey, cancellationToken);

            this.logger.LogInformation("Cache data with cacheKey: {CacheKey} removed.", this.CacheKey);
        }
    }
}
