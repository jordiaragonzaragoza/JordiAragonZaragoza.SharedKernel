namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    public abstract class BaseEventHandler<TEvent> : IInMemoryEventHandler<TEvent>
        where TEvent : IEvent
    {
        public Task Handle(TEvent notification, CancellationToken cancellationToken)
            => this.HandleAsync(notification, cancellationToken);

        public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}