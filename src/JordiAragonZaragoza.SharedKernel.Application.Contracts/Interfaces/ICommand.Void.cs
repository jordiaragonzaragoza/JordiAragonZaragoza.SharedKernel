namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.Result;
    using MediatR;

    /// <summary>
    /// Pure DDD transactional command. Commands will not use response operation check impure alternative <see cref="ICommand{TResponse}"/>.
    /// </summary>
    public interface ICommand : IRequest<Result>, IBaseCommand
    {
    }
}