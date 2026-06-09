namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore
{
    using System;
    using System.Diagnostics;
    using Ardalis.SmartEnum;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    /// <summary>
    /// Persisted as the KurrentDB event metadata blob.
    /// Combines business context (ExecutionContext) with the
    /// W3C TraceContext propagation fields needed by OpenTelemetry.
    /// </summary>
    public sealed record EventStoreMetadata
    {
        private EventStoreMetadata(
            string actorId,
            string actorType,
            string executor,
            string executorType,
            Guid correlationId,
            Guid? causationId,
            Guid tenantId,
            Guid? partitionId,
            Guid? domainId,
            DateTimeOffset dateOccurredOnUtc,
            string? traceParent,
            string? traceState)
        {
            this.ActorId = actorId;
            this.ActorType = actorType;
            this.Executor = executor;
            this.ExecutorType = executorType;
            this.CorrelationId = correlationId;
            this.CausationId = causationId;
            this.TenantId = tenantId;
            this.PartitionId = partitionId;
            this.DomainId = domainId;
            this.DateOccurredOnUtc = dateOccurredOnUtc;
            this.TraceParent = traceParent;
            this.TraceState = traceState;
        }

        public string ActorId { get; }

        public string ActorType { get; }

        public string Executor { get; }

        public string ExecutorType { get; }

        public Guid CorrelationId { get; }

        public Guid? CausationId { get; }

        public Guid TenantId { get; }

        public Guid? PartitionId { get; }

        public Guid? DomainId { get; }

        public DateTimeOffset DateOccurredOnUtc { get; }

        public string? TraceParent { get; }

        public string? TraceState { get; }

        public bool IsValid()
            => !string.IsNullOrWhiteSpace(this.ActorId)
            && !string.IsNullOrWhiteSpace(this.ActorType)
            && !string.IsNullOrWhiteSpace(this.Executor)
            && !string.IsNullOrWhiteSpace(this.ExecutorType)
            && this.CorrelationId != Guid.Empty
            && this.TenantId != Guid.Empty;

        public static EventStoreMetadata From(
            ExecutionContext executionContext,
            DateTimeOffset dateOccurredOnUtc)
        {
            ArgumentNullException.ThrowIfNull(executionContext);

            var activity = Activity.Current;
            string? traceParent = null;
            string? traceState = null;

            if (activity is not null)
            {
                // W3C format: 00-{traceId}-{spanId}-{flags}
                traceParent = $"00-{activity.TraceId}-{activity.SpanId}-" +
                    $"{(activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}";
                traceState = activity.TraceStateString;
            }

            return new EventStoreMetadata(
                actorId: executionContext.ActorId,
                actorType: executionContext.ActorType.Name,
                executor: executionContext.Executor,
                executorType: executionContext.ExecutorType.Name,
                correlationId: executionContext.CorrelationId,
                causationId: executionContext.CausationId,
                tenantId: executionContext.ScopeContext.TenantId,
                partitionId: executionContext.ScopeContext.PartitionId,
                domainId: executionContext.ScopeContext.DomainId,
                dateOccurredOnUtc: dateOccurredOnUtc,
                traceParent: traceParent,
                traceState: traceState);
        }

        public ExecutionContext? ToProcessingExecutionContext(Guid causationId)
        {
            if (!this.IsValid())
            {
                return null;
            }

            try
            {
                var actorType = SmartEnum<ActorType, int>.FromName(this.ActorType, ignoreCase: true);
                var executorType = SmartEnum<ExecutorType, int>.FromName(this.ExecutorType, ignoreCase: true);
                var scopeContext = new ScopeContext(this.TenantId, this.PartitionId, this.DomainId);

                return new ExecutionContext(
                    this.ActorId,
                    actorType,
                    this.Executor,
                    executorType,
                    this.CorrelationId,
                    causationId,
                    scopeContext);
            }
            catch (Exception ex) when (ex is SmartEnumNotFoundException or ArgumentException)
            {
                return null;
            }
        }
    }
}