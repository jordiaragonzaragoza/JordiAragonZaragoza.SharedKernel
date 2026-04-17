namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    public interface IExecutionContextService
    {
        ExecutionContext? CurrentContext { get; }

        void SetExecutionContext(ExecutionContext executionContext);

        void ClearExecutionContext();
    }
}