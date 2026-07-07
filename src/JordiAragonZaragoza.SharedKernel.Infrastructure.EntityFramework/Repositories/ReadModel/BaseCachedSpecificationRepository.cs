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

        /// <summary>
        /// Gets the base prefix for all cache entries of this read model type.
        /// Uses the short type name (no namespace) so that renaming or moving
        /// namespaces does not leave orphaned entries in the distributed cache.
        /// All per-parameter keys are registered under this prefix so that a
        /// single RemoveByPrefixAsync call invalidates every cached variant.
        /// </summary>
        public string CacheKey => typeof(TReadModel).Name;

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

        /// <summary>
        /// Updates the read model in the database.
        /// Cache is invalidated before the write so concurrent reads go to the DB
        /// and get fresh data once the write completes.
        /// The entity is reloaded from the DB with tracking active before saving
        /// because the DbContext is configured globally with NoTracking, and the
        /// incoming entity may have come from cache (detached). Without reloading,
        /// EF cannot obtain the correct PostgreSQL xmin concurrency token and the
        /// UPDATE fails with DbUpdateConcurrencyException (0 rows affected).
        /// Trade-off: this costs an extra roundtrip per update. Acceptable given
        /// the read-heavy nature of read models and the NoTracking design choice.
        /// </summary>
        /// <param name="entity">The read model entity to update. Must have a valid Id and match an existing DB record.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of records updated (1 if successful).</returns>
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

            var cached = await this.CacheGetAsync<TReadModel>(cacheKeyId, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.GetByIdAsync(id, cancellationToken);

            await this.CacheSetAsync(cacheKeyId, response, cancellationToken);
            return response;
        }

        public override async Task<TReadModel?> FirstOrDefaultAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.CacheGetAsync<TReadModel>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.FirstOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.cacheService.GetAsync<TResult>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
            }

            var response = await base.FirstOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<TReadModel?> SingleOrDefaultAsync(ISingleResultSpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.CacheGetAsync<TReadModel>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.SingleOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
            where TResult : default
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.cacheService.GetAsync<TResult>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
            }

            var response = await base.SingleOrDefaultAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<List<TReadModel>> ListAsync(CancellationToken cancellationToken = default)
        {
            var cached = await this.CacheGetListAsync<TReadModel>(this.CacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.ListAsync(cancellationToken);

            await this.CacheSetListAsync(this.CacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<List<TReadModel>> ListAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.CacheGetListAsync<TReadModel>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.ListAsync(specification, cancellationToken);

            await this.CacheSetListAsync(cacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<List<TResult>> ListAsync<TResult>(ISpecification<TReadModel, TResult> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.CacheGetListAsync<TResult>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var response = await base.ListAsync(specification, cancellationToken);

            await this.CacheSetListAsync(cacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<int> CountAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.cacheService.GetAsync<int>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
            }

            var response = await base.CountAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var cached = await this.cacheService.GetAsync<int>(this.CacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", this.CacheKey);
                return cached.Value;
            }

            var response = await base.CountAsync(cancellationToken);

            await this.CacheSetAsync(this.CacheKey, response, cancellationToken);
            return response;
        }

        public override async Task<bool> AnyAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var cacheKey = this.ResolveCacheKey(specification);

            var cached = await this.cacheService.GetAsync<bool>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
            }

            var response = await base.AnyAsync(specification, cancellationToken);

            await this.CacheSetAsync(cacheKey, response, cancellationToken);

            return response;
        }

        public override async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            var cached = await this.cacheService.GetAsync<bool>(this.CacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", this.CacheKey);
                return cached.Value;
            }

            var response = await base.AnyAsync(cancellationToken);

            await this.CacheSetAsync(this.CacheKey, response, cancellationToken);

            return response;
        }

        /// <summary>
        /// Returns true when <paramref name="source"/> and <paramref name="destination"/>
        /// share identical values for all <paramref name="keyProperties"/>.
        /// Uses reflection because owned items are not tracked at this point and their
        /// CLR property values are the only reliable source of identity.
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
        /// Resolves the final cache key for a specification.
        /// Always prefixed with the read model type name so that
        /// RemoveByPrefixAsync(this.CacheKey) correctly invalidates all entries.
        /// <para>
        /// If the specification has CacheEnabled (i.e. WithCacheKey was called),
        /// its parameter-aware CacheKey is used — safe for parameterized specs.
        /// Otherwise falls back to the specification's short type name, which is
        /// only correct for parameterless specifications that always return the
        /// same result set. Parameterized specs without WithCacheKey will collide.
        /// </para>
        /// </summary>
        private string ResolveCacheKey(ISpecification<TReadModel> specification)
            => specification.CacheEnabled
                ? $"{this.CacheKey}_{specification.CacheKey}"
                : $"{this.CacheKey}_{specification.GetType().Name}";

        /// <summary>
        /// Overload for projected specifications. Both ISpecification&lt;T, TResult&gt;
        /// and ISpecification&lt;T&gt; share the same CacheKey/CacheEnabled contract
        /// via the base Specification&lt;T&gt; class, so the logic is identical.
        /// </summary>
        private string ResolveCacheKey<TResult>(ISpecification<TReadModel, TResult> specification)
            => specification.CacheEnabled
                ? $"{this.CacheKey}_{specification.CacheKey}"
                : $"{this.CacheKey}_{specification.GetType().Name}";

        private async Task ReloadAndApplyChangesAsync(TReadModel entity, CancellationToken cancellationToken)
        {
            var entityId = this.GetEntityId(entity);

            var trackedEntity = await this.DbContext.Set<TReadModel>()
                .AsTracking()
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Entity of type {typeof(TReadModel).Name} with id {entityId} was not found in the database during UpdateAsync.");

            this.DbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);

            this.ApplyOwnedCollectionChanges(trackedEntity, entity);
        }

        /// <summary>
        /// Extracts the primary key value from the entity using EF Core metadata.
        /// Avoids hardcoding a property name so the method works for any TReadModel.
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
        /// Reconciles owned collection navigations between the tracked DB entity
        /// (<paramref name="destination"/>) and the incoming detached entity
        /// (<paramref name="source"/>).
        /// <para>
        /// Items present only in <paramref name="source"/> are marked Added.
        /// Items present only in <paramref name="destination"/> are marked Deleted.
        /// Items present in both are left untouched (scalar values are already
        /// covered by SetValues on the parent entry).
        /// </para>
        /// <para>
        /// Collection detection uses the FK metadata (non-unique ownership FK) instead
        /// of INavigationBase.IsCollection to avoid MissingMethodException on EF Core
        /// builds where that property is not available.
        /// </para>
        /// <para>
        /// Identity matching uses reflection on the CLR properties corresponding to
        /// the owned entity's configured primary key. Reflection is limited to write
        /// operations and is not on the read hot path.
        /// </para>
        /// </summary>
        private void ApplyOwnedCollectionChanges(TReadModel destination, TReadModel source)
        {
            var destinationEntry = this.DbContext.Entry(destination);
            var sourceEntry = this.DbContext.Entry(source);

            foreach (var destNavigation in destinationEntry.Navigations)
            {
                var fk = (destNavigation.Metadata as INavigation)?.ForeignKey;
                if (fk is null || fk.IsUnique || !fk.IsOwnership)
                {
                    continue;
                }

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

                foreach (var destItem in destList.Where(d => !sourceList.Any(s => OwnedKeysMatch(s, d, keyProperties))))
                {
                    this.DbContext.Entry(destItem).State = EntityState.Deleted;
                }

                var ownerFk = ownedEntityType.GetForeignKeys().First(f => f.IsOwnership);
                var ownerPkValue = destinationEntry
                    .Property(ownerFk.PrincipalKey.Properties[0].Name).CurrentValue;

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
            var cached = await this.cacheService.GetAsync<T>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
            }

            return default;
        }

        private async Task<List<TIn>?> CacheGetListAsync<TIn>(string cacheKey, CancellationToken cancellationToken)
        {
            var cached = await this.cacheService.GetAsync<List<TIn>>(cacheKey, cancellationToken);
            if (!cached.IsNull && cached.HasValue)
            {
                this.logger.LogInformation("Fetch data from cache with cacheKey: {CacheKey}", cacheKey);
                return cached.Value;
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