namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    public interface IExecutionContextService
    {
        ExecutionContext? CurrentContext { get; }

        /// <summary>
        /// Sets the context for the first time. Throws if one already exists.
        /// Use in HTTP middleware, where a double assignment is always an error.
        /// </summary>
        /// <param name="executionContext">The context to set.</param>
        void SetExecutionContext(ExecutionContext executionContext);

        /// <summary>
        /// Overrides the existing context.
        /// Use in workers and message consumers, where each message
        /// has its own independent context.
        /// </summary>
        /// <param name="executionContext">The context to override with.</param>
        void OverrideExecutionContext(ExecutionContext executionContext);

        void ClearExecutionContext();
    }
}