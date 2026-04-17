namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public sealed record class ExecutionContext
    {
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
                throw new ArgumentException("ActorId is required");
            }

            if (string.IsNullOrWhiteSpace(executor))
            {
                throw new ArgumentException("Executor is required");
            }

            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException("CorrelationId is required");
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
    }
}