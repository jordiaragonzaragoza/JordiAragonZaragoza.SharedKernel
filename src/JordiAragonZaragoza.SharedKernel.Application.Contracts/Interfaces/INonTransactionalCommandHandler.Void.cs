namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.Result;
    using MediatR;

    public interface INonTransactionalCommandHandler<TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : INonTransactionalCommand
    {
    }
}