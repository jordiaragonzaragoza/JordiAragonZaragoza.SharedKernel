namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore
{
    using System;
    using System.Collections.Concurrent;

    public class StreamNameMapper
    {
        private static readonly StreamNameMapper Instance = new();

        private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

        public static void AddCustomMap<TStream>(string mappedStreamName)
        {
            AddCustomMap(typeof(TStream), mappedStreamName);
        }

        public static void AddCustomMap(Type streamType, string mappedStreamName)
        {
            _ = Instance.typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
        }

        public static string ToStreamPrefix<TStream>()
        {
            return ToStreamPrefix(typeof(TStream));
        }

        public static string ToStreamPrefix(Type streamType)
        {
            return Instance.typeNameMap.GetOrAdd(streamType, static type =>
            {
                var modulePrefix = type.Namespace!;

                var namespaceParts = modulePrefix.Split(".");
                if (namespaceParts.Length > 2)
                {
                    modulePrefix = namespaceParts[2];
                }
                else if (namespaceParts.Length >= 1)
                {
                    modulePrefix = namespaceParts[0];
                }

                return $"{modulePrefix}_{type.Name}";
            });
        }

        public static string ToStreamId<TStream>(object aggregateId, object? tenantId = null)
        {
            return ToStreamId(typeof(TStream), aggregateId, tenantId);
        }

        /// <summary>
        /// Generates a stream id in the canonical `{category}-{aggregateId}` format.
        /// It can be expanded to the `{module}_{streamType}-{tenantId}_{aggregateId}` format.
        /// </summary>
        /// <param name="streamType">The stream type based on aggregate type.</param>
        /// <param name="aggregateId">The aggregate Id.</param>
        /// <param name="tenantId">The Tenant Id.</param>
        /// <returns>The Stream Id.</returns>
        public static string ToStreamId(Type streamType, object aggregateId, object? tenantId = null)
        {
            var tenantPrefix = tenantId != null ? $"{tenantId}_" : string.Empty;
            var streamCategory = ToStreamPrefix(streamType);

            // (Out-of-the box, the category projection treats anything before a `-` separator as the category name)
            // For this reason, we place the "{tenantId}_" bit (if present) on the right hand side of the '-'
            return $"{streamCategory}-{tenantPrefix}{aggregateId}";
        }
    }
}