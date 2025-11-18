namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IIntegrationEventBus
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IIntegrationEvent;
    }
}