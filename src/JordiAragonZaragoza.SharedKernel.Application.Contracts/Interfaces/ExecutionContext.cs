namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public sealed record class ExecutionContext(
        string ActorId,
        string ActorType,
        Guid CorrelationId,
        Guid? CausationId);
}