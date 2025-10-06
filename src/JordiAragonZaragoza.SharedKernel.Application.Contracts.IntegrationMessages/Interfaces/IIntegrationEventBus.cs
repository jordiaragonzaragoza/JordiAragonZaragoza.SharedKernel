namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces;

    public interface IIntegrationEventBus
    {
        Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IIntegrationEvent;
    }
}