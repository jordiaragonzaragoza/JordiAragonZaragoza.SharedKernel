namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context.User
{
    using System;
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using ExecutionContext = JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces.ExecutionContext;

    public class ExecutionContextService : IExecutionContextService
    {
        private static readonly AsyncLocal<ExecutionContext?> AsyncUserContext = new();

        public ExecutionContext CurrentContext =>
            AsyncUserContext.Value ?? throw new InvalidOperationException("ExecutionContext has not been set.");

        public void SetExecutionContext(string actorId, string actorType, Guid correlationId, Guid? causationId = default)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException("ActorId cannot be null or whitespace.", nameof(actorId));
            }

            if (string.IsNullOrWhiteSpace(actorType))
            {
                throw new ArgumentException("ActorType cannot be null or whitespace.", nameof(actorType));
            }

            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException("CorrelationId cannot be empty.", nameof(correlationId));
            }

            if (AsyncUserContext.Value is not null)
            {
                throw new InvalidOperationException("ExecutionContext has already been set for the current context.");
            }

            AsyncUserContext.Value = new ExecutionContext(actorId, actorType, correlationId, causationId);
        }
    }
}