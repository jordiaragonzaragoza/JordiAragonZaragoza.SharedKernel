namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public interface IExecutionContextService
    {
        ExecutionContext CurrentContext { get; }

        void SetExecutionContext(
            string actorId,
            string actorType,
            Guid correlationId,
            Guid tenantId,
            Guid? partitionId = default,
            Guid? domainId = default,
            Guid? causationId = default);
    }
}