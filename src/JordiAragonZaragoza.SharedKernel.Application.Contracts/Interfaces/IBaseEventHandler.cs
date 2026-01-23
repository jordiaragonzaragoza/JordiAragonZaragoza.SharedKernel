namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    /// <summary>
    /// Defines a common contract for handling events,
    /// regardless of the underlying event bus implementation (in-memory or external).
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle, which must implement <see cref="IEvent"/>.</typeparam>
    public interface IBaseEventHandler<in TEvent>
        where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}