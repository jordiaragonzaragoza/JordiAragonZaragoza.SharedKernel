namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    /// <summary>
    /// Provides a mechanism for sending integration commands through a messaging bus.
    /// <para>
    /// ⚠️ Warning: Using this interface may introduce strong coupling between different bounded contexts,
    /// which can be considered an anti-pattern in distributed architectures based on DDD.
    /// It is recommended to evaluate alternatives such as integration events to reduce coupling.
    /// </para>
    /// </summary>
    public interface IIntegrationCommandBus
    {
        Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, IIntegrationCommand;

        Task<Result<TResponse>> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, IIntegrationCommand
            where TResponse : notnull;
    }
}