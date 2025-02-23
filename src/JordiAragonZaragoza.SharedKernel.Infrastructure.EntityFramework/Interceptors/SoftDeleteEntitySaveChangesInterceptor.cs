namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class SoftDeleteEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void UpdateEntities(DbContext? context)
        {
            if (context == null)
            {
                return;
            }

            var entries = context.ChangeTracker.Entries()
                .Where(static entity => entity.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                // Checks if the entity has the shadow property "IsDeleted"
                var entityType = context.Model.FindEntityType(entry.Entity.GetType());
                var isDeletedProperty = entityType?.FindProperty("IsDeleted");

                if (isDeletedProperty != null)
                {
                    entry.State = EntityState.Modified;
                    entry.CurrentValues["IsDeleted"] = true;
                }
            }
        }
    }
}