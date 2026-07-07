namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Application.ReadModels;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class TenantReadModelSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IExecutionContextService executionContextService;

        public TenantReadModelSaveChangesInterceptor(IExecutionContextService executionContextService)
        {
            this.executionContextService = executionContextService
                ?? throw new ArgumentNullException(nameof(executionContextService));
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            ArgumentNullException.ThrowIfNull(eventData);
            this.ApplyScopeToEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventData);
            this.ApplyScopeToEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyScopeToEntities(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var currentContext = this.executionContextService.CurrentContext;

            // Sync CurrentTenantId for HasQueryFilter consistency.
            if (context is BaseReadModelContext readModelContext)
            {
                readModelContext.CurrentTenantId = currentContext?.ScopeContext.TenantId;
            }

            if (currentContext is null)
            {
                return;
            }

            var addedEntries = context.ChangeTracker
                .Entries<IScopedReadModel>()
                .Where(e => e.State == EntityState.Added);

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (var entry in addedEntries)
            {
                // Only set if Scope is uninitialized — projectors that
                // explicitly set Scope from event metadata take precedence.
                // A new ScopeInfo instance is created per entity: EF Core's
                // OwnsOne tracking cannot resolve the same owned-type
                // reference across multiple owner entities in a single
                // SaveChanges batch.
                if (entry.Entity is BaseReadModel baseReadModel
                    && (baseReadModel.Scope is null || baseReadModel.Scope.TenantId == Guid.Empty))
                {
                    baseReadModel.Scope = ScopeInfo.From(currentContext.ScopeContext);
                }
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
        }
    }
}