namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.Result;
    using MediatR;

    /// <summary>
    /// Command that operate on a single aggregate. Aggregate boundary is expected to be the unit of work.
    /// ⚠️ Warning. Using this type of commands domain events must be created/published by database infraestructure.
    /// Command with response operation. To see pure DDD commands check <see cref="INonTransactionalCommand"/>.
    /// </summary>
    /// <typeparam name="TResponse">The response operation.</typeparam>
    public interface INonTransactionalCommand<TResponse> : IRequest<Result<TResponse>>
        where TResponse : notnull
    {
    }
}