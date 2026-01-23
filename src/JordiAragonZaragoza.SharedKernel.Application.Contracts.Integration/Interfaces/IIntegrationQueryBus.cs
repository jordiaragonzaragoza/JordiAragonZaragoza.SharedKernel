namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    /// <summary>
    /// Provides a mechanism for sending integration querys through a messaging bus.
    /// <para>
    /// ⚠️ Warning: Using this interface may introduce strong coupling between different bounded contexts,
    /// which can be considered an anti-pattern in distributed architectures based on DDD.
    /// It is recommended to evaluate alternatives such as integration events to reduce coupling.
    /// </para>
    /// </summary>
    public interface IIntegrationQueryBus
    {
        Task<Result<TResponse>> SendAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : class, IIntegrationQuery
            where TResponse : notnull;
    }
}