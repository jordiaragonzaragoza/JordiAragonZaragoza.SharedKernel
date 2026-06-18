namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore
{
    using System.Diagnostics;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Contracts;

    /// <summary>
    /// Restores the W3C TraceContext stored in event metadata so that
    /// downstream spans (projections, reactions) appear as children of
    /// the original write span.
    /// </summary>
    public static class EventStoreActivityRestorer
    {
        private static readonly ActivitySource ActivitySource =
            new(InfrastructureActivitySources.EventStore);

        /// <summary>
        /// Creates a new Activity linked to the trace that produced the event.
        /// Dispose the returned activity when processing is complete.
        /// </summary>
        /// <param name="metadata">The event metadata containing the trace context.</param>
        /// <param name="operationName">The name of the operation for the new activity.</param>
        /// <returns>A new Activity linked to the original trace, or a standalone Activity if.</returns>
        public static Activity? RestoreFrom(EventStoreMetadata? metadata, string operationName)
        {
            if (metadata?.TraceParent is null)
            {
                return ActivitySource.StartActivity(operationName);
            }

            // Parse the W3C traceparent header.
            if (!ActivityContext.TryParse(metadata.TraceParent, metadata.TraceState, out var parentContext))
            {
                return ActivitySource.StartActivity(operationName);
            }

            // Start a new span that is a *child* of the original write span.
            return ActivitySource.StartActivity(
                operationName,
                ActivityKind.Consumer,
                parentContext);
        }
    }
}