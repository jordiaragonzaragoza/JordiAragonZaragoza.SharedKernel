namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public sealed record class ExecutionContext
    {
        // Known prefixes that identify the type of actor.
        // Keeping them here centralises the convention so that both the
        // middleware and any worker/consumer that reconstructs the context
        // from message metadata use the same rule.
        private const string UserPrefix = "user:";
        private const string ServicePrefix = "service:";
        private const string JobPrefix = "job:";
        private const string ExternalPrefix = "external:";

        private static readonly string[] KnownActorPrefixes = [UserPrefix, ServicePrefix, JobPrefix, ExternalPrefix];

        public ExecutionContext(
            string actorId,
            ActorType actorType,
            string executor,
            ExecutorType executorType,
            Guid correlationId,
            Guid? causationId,
            ScopeContext scopeContext)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException("ActorId is required.", nameof(actorId));
            }

            if (!IsValidActorIdFormat(actorId))
            {
                throw new ArgumentException(
                    $"ActorId must start with a known prefix ({string.Join(", ", KnownActorPrefixes)}).",
                    nameof(actorId));
            }

            if (string.IsNullOrWhiteSpace(executor))
            {
                throw new ArgumentException("Executor is required", nameof(executor));
            }

            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException("CorrelationId must not be empty.", nameof(correlationId));
            }

            this.ActorId = actorId;
            this.ActorType = actorType ?? throw new ArgumentNullException(nameof(actorType));
            this.Executor = executor;
            this.ExecutorType = executorType ?? throw new ArgumentNullException(nameof(executorType));
            this.CorrelationId = correlationId;
            this.CausationId = causationId;
            this.ScopeContext = scopeContext ?? throw new ArgumentNullException(nameof(scopeContext));
        }

        public string ActorId { get; }

        public ActorType ActorType { get; }

        public string Executor { get; }

        public ExecutorType ExecutorType { get; }

        public Guid CorrelationId { get; }

        public Guid? CausationId { get; }

        public ScopeContext ScopeContext { get; }

        public static string CreateUserActorId(Guid userId)
            => $"{UserPrefix}{userId}";

        public static string CreateExternalActorId(string externalActorName)
        {
            if (string.IsNullOrWhiteSpace(externalActorName))
            {
                throw new ArgumentException("External actor name is required", nameof(externalActorName));
            }

            return $"{ExternalPrefix}{externalActorName}";
        }

        public static string CreateServiceActorId(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name is required", nameof(serviceName));
            }

            return $"{ServicePrefix}{serviceName}";
        }

        public static string CreateJobActorId(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new ArgumentException("Job name is required", nameof(jobName));
            }

            return $"{JobPrefix}{jobName}";
        }

        public static bool IsValidActorIdFormat(string actorId)
        {
            ArgumentNullException.ThrowIfNull(actorId);

            foreach (var prefix in KnownActorPrefixes)
            {
                if (actorId.StartsWith(prefix, StringComparison.Ordinal) && actorId.Length > prefix.Length)
                {
                    return true;
                }
            }

            return false;
        }

        public Guid GetUserActorId()
        {
            if (this.ActorType != ActorType.User)
            {
                throw new InvalidOperationException("ActorType is not User.");
            }

            var raw = this.ActorId.Substring(UserPrefix.Length);
            return Guid.Parse(raw);
        }
    }
}