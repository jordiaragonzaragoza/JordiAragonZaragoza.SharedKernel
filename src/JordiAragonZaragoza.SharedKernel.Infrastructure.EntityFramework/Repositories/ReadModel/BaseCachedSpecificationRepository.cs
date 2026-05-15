namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Specification;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.Extensions.Logging;

    public abstract class BaseCachedSpecificationRepository<TReadModel> : BaseReadRepository<TReadModel>, ICachedSpecificationRepository<TReadModel, Guid>
        where TReadModel : class, IReadModel
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<BaseCachedSpecificationRepository<TReadModel>> logger;

        protected BaseCachedSpecificationRepository(
            BaseReadModelContext dbContext,
            ILogger<BaseCachedSpecificationRepository<TReadModel>> logger,
            ICacheService cacheService)
            : base(dbContext)
        {
            this.cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string CacheKey => $"{typeof(TReadModel)}";

        public override async Task<TReadModel> AddAsync(TReadModel entity, CancellationToken cancellationToken = default)
        {
            var response = await base.AddAsync(entity, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
            return response;
        }

        public override async Task<IEnumerable<TReadModel>> AddRangeAsync(IEnumerable<TReadModel> entities, CancellationToken cancellationToken = default)
        {
            var response = await base.AddRangeAsync(entities, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);
            return response;
        }

        public override async Task<int> UpdateAsync(TReadModel entity, CancellationToken cancellationToken = default)
        {
            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);

            await this.ReloadAndApplyChangesAsync(entity, cancellationToken);

            await this.DbContext.SaveChangesAsync(cancellationToken);

            return 1;
        }

        public override async Task<int> UpdateRangeAsync(IEnumerable<TReadModel> entities, CancellationToken cancellationToken = default)
        {
            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);

            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                await this.ReloadAndApplyChangesAsync(entity, cancellationToken);
            }

            await this.DbContext.SaveChangesAsync(cancellationToken);

            return entityList.Count;
        }

        public override async Task<int> DeleteAsync(TReadModel entity, CancellationToken cancellationToken = default)
        {
            var result = await base.DeleteAsync(entity, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);

            return result;
        }

        public override async Task<int> DeleteRangeAsync(IEnumerable<TReadModel> entities, CancellationToken cancellationToken = default)
        {
            var result = await base.DeleteRangeAsync(entities, cancellationToken);

            await this.CacheServiceRemoveByPrefixAsync(cancellationToken);

            return result;
        }

        public override async Task<TReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKeyId = $"{this.CacheKey}_{id}";

            var cacheResponse = await this.CacheGetAsync<TReadModel>(cacheKeyId, cancellationToken);
            if (cacheResponse != null)
            {
                return cacheResponse;
            }

            var response = await base.GetByIdAsync(id, cancellationToken);

            await this.CacheSetAsync(cacheKeyId, response, cancellationToken);
            return response;
        }

        public override async Task<TReadModel?> FirstOrDefaultAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";

            var cachedResponse = await this.CacheGetAsync<TReadModel>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.FirstOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

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

        public override async Task<TReadModel?> SingleOrDefaultAsync(ISingleResultSpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.CacheGetAsync<TReadModel>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.SingleOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKeySpecification, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

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

        public override async Task<List<TReadModel>> ListAsync(CancellationToken cancellationToken = default)
        {
            var cachedResponse = await this.CacheGetListAsync<TReadModel>(this.CacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.ListAsync(cancellationToken);

            await this.CacheSetListAsync(this.CacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<List<TReadModel>> ListAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

            var cacheKeySpecification = $"{this.CacheKey}_{specification.GetType().FullName}";
            var cachedResponse = await this.CacheGetListAsync<TReadModel>(cacheKeySpecification, cancellationToken);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await base.ListAsync(specification, cancellationToken);

            await this.CacheSetListAsync(cacheKeySpecification, response, cancellationToken);
            return response;
        }

        public override async Task<List<TResult>> ListAsync<TResult>(ISpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

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

        public override async Task<int> CountAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

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

        public override async Task<bool> AnyAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

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

        /// <summary>
        /// Returns true when <paramref name="source"/> and <paramref name="destination"/>
        /// share the same values for all <paramref name="keyProperties"/>.
        /// </summary>
        private static bool OwnedKeysMatch(
            object source,
            object destination,
            IReadOnlyList<IProperty> keyProperties)
        {
            var srcType = source.GetType();
            var dstType = destination.GetType();

            return keyProperties.All(kp =>
                Equals(
                    srcType.GetProperty(kp.Name)?.GetValue(source),
                    dstType.GetProperty(kp.Name)?.GetValue(destination)));
        }

        /// <summary>
        /// Reloads the entity from the DB with tracking, copies scalar values from
        /// <paramref name="entity"/>, then reconciles all owned collections.
        /// Extracted to avoid duplication between UpdateAsync and UpdateRangeAsync.
        /// </summary>
        private async Task ReloadAndApplyChangesAsync(TReadModel entity, CancellationToken cancellationToken)
        {
            var entityId = this.GetEntityId(entity);

            // Load a fresh tracked copy so EF has the correct concurrency token (xmin).
            // OwnsMany navigations are included automatically when using AsTracking()
            // with the owned-entity pattern.
            var trackedEntity = await this.DbContext.Set<TReadModel>()
                .AsTracking()
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Entity of type {typeof(TReadModel).Name} with id {entityId} was not found in the database during UpdateAsync.");

            // Copy scalar property values from the incoming (possibly detached) entity.
            this.DbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);

            // Reconcile owned collections.
            this.ApplyOwnedCollectionChanges(trackedEntity, entity);
        }

        /// <summary>
        /// Extracts the primary key (Id) from the entity using EF Core metadata.
        /// </summary>
        private Guid GetEntityId(TReadModel entity)
        {
            var entry = this.DbContext.Entry(entity);
            var keyValue = entry.Metadata
                .FindPrimaryKey()!
                .Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .Single();

            return (Guid)keyValue!;
        }

        /// <summary>
        /// Reconciles owned collection navigations between a tracked DB entity and
        /// the incoming (detached) entity. Items present only in <paramref name="source"/>
        /// are inserted; items present only in <paramref name="destination"/> are deleted.
        /// Identity is determined by the owned entity's configured primary key.
        /// </summary>
        private void ApplyOwnedCollectionChanges(TReadModel destination, TReadModel source)
        {
            var destinationEntry = this.DbContext.Entry(destination);
            var sourceEntry = this.DbContext.Entry(source);

            foreach (var destNavigation in destinationEntry.Navigations)
            {
                // Use the EF metadata to determine whether this is an owned collection.
                // An owned collection has a non-unique FK (IsUnique == false on the FK).
                // This avoids relying on INavigationBase.IsCollection which was introduced
                // in a later EF Core minor and causes MissingMethodException on older builds.
                var fk = (destNavigation.Metadata as INavigation)?.ForeignKey;
                if (fk is null || fk.IsUnique || !fk.IsOwnership)
                {
                    continue;
                }

                // Load current children from the DB (already tracked via AsTracking above,
                // but Load() is a no-op if already populated).
                destNavigation.Load();

                var sourceNavigation = sourceEntry.Navigations
                    .FirstOrDefault(n => n.Metadata.Name == destNavigation.Metadata.Name);

                if (sourceNavigation?.CurrentValue is not System.Collections.IEnumerable sourceItems)
                {
                    continue;
                }

                var sourceList = sourceItems.Cast<object>().ToList();
                var destList = (destNavigation.CurrentValue as System.Collections.IEnumerable)
                    ?.Cast<object>().ToList()
                    ?? [];

                var ownedEntityType = destNavigation.Metadata.TargetEntityType;
                var keyProperties = ownedEntityType.FindPrimaryKey()!.Properties;

                // Delete children removed from the source.
                foreach (var destItem in destList.Where(d => !sourceList.Any(s => OwnedKeysMatch(s, d, keyProperties))))
                {
                    this.DbContext.Entry(destItem).State = EntityState.Deleted;
                }

                // Insert children added in the source.
                // We set the FK back-reference so EF can generate the INSERT correctly.
                var ownerFk = ownedEntityType.GetForeignKeys().First(f => f.IsOwnership);
                var ownerPkPropertyName = ownerFk.PrincipalKey.Properties[0].Name;
                var ownerPkValue = destinationEntry.Property(ownerPkPropertyName).CurrentValue;

                foreach (var srcItem in sourceList.Where(s => !destList.Any(d => OwnedKeysMatch(s, d, keyProperties))))
                {
                    var itemEntry = this.DbContext.Entry(srcItem);
                    itemEntry.State = EntityState.Added;

                    foreach (var fkProp in ownerFk.Properties)
                    {
                        itemEntry.Property(fkProp.Name).CurrentValue = ownerPkValue;
                    }
                }
            }
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

            this.logger.LogInformation("Set data to cache with cacheKey: {CacheKey}", cacheKey);
        }

        private async Task CacheSetListAsync<TIn>(string cacheKey, List<TIn> response, CancellationToken cancellationToken)
        {
            await this.cacheService.SetAsync(cacheKey, response, cancellationToken: cancellationToken);

            this.logger.LogInformation("Set data to cache with cacheKey: {CacheKey}", cacheKey);
        }

        private async Task CacheServiceRemoveByPrefixAsync(CancellationToken cancellationToken)
        {
            await this.cacheService.RemoveByPrefixAsync(this.CacheKey, cancellationToken);

            this.logger.LogInformation("Cache data with cacheKey: {CacheKey} removed.", this.CacheKey);
        }
    }
}