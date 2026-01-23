namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    /// <summary>
    /// Marker interface to allow generic type constraints of objects implementing <see cref="ICommand"/> or <see cref="ICommand{TResponse}"/>.
    /// </summary>
    public interface ITransactionalCommand
    {
    }
}