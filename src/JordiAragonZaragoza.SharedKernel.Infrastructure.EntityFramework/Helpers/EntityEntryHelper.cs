namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public static class EntityEntryHelper
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));

            return entry.References.Any(static referenceEntry =>
                referenceEntry.TargetEntry != null &&
                referenceEntry.TargetEntry.Metadata.IsOwned() &&
                (referenceEntry.TargetEntry.State == EntityState.Added || referenceEntry.TargetEntry.State == EntityState.Modified));
        }
    }
}