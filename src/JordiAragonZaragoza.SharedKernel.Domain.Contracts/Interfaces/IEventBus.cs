namespace JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    public interface IEventBus
    {
        Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
    }
}