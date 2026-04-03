namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public interface IExecutionContextService
    {
        ExecutionContext CurrentContext { get; }

        void SetExecutionContext(string actorId, string actorType, Guid correlationId, Guid? causationId = default);
    }
}