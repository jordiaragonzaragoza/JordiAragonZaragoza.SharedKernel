namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.Interfaces.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;

    /// <summary>
    /// Defines a common contract for handling integration messages,
    /// regardless of the underlying event bus implementation.
    /// </summary>
    /// <typeparam name="TIntegrationMessage">The type of integration message to handle, which must implement <see cref="IIntegrationMessage"/>.</typeparam>
    public interface IBaseIntegrationMessageHandler<in TIntegrationMessage>
        where TIntegrationMessage : IIntegrationMessage
    {
        Task HandleAsync(TIntegrationMessage integrationMessage, CancellationToken cancellationToken);
    }
}