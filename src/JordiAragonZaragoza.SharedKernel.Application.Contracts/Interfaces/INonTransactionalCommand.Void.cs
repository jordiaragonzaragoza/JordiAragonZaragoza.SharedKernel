namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.Result;
    using MediatR;

    /// <summary>
    /// Commands that operate on a single aggregate. Aggregate boundary is expected to be the unit of work.
    /// ⚠️ Warning. Using this type of commands domain events must be created/published by database infraestructure.
    /// Pure DDD command. Commands will not use response operation check impure alternative <see cref="INonTransactionalCommand{TResponse}"/>.
    /// </summary>
    public interface INonTransactionalCommand : IRequest<Result>
    {
    }
}