namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context
{
    using System;
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using ExecutionContext = JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces.ExecutionContext;

    public class ExecutionContextService : IExecutionContextService
    {
        private static readonly AsyncLocal<ExecutionContext?> AsyncUserContext = new();

        public ExecutionContext? CurrentContext => AsyncUserContext.Value;

        public void SetExecutionContext(ExecutionContext executionContext)
        {
            ArgumentNullException.ThrowIfNull(executionContext);

            if (AsyncUserContext.Value is not null)
            {
                throw new InvalidOperationException("ExecutionContext has already been set for the current context.");
            }

            AsyncUserContext.Value = executionContext;
        }

        public void ClearExecutionContext()
        {
            AsyncUserContext.Value = null;
        }
    }
}