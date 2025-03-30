namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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
        // TODO: Complete.
        /*Task SendQueryAsync<T>(T Query, Uri endpointAddress, CancellationToken cancellationToken = default)
            where T : class, IIntegrationQuery;*/
    }
}