namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    public abstract class BaseEventHandler<TEvent> : IInMemoryEventHandler<TEvent>
        where TEvent : IEvent
    {
        private static readonly ActivitySource ActivitySource =
            new(ApplicationActivitySources.Handlers);

        public async Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            using var activity = ActivitySource.StartActivity(
                $"EventHandler: {this.GetType().Name}",
                ActivityKind.Internal);

            activity?.SetTag("event.type", typeof(TEvent).Name);
            activity?.SetTag("handler.type", this.GetType().FullName);

            await this.HandleAsync(notification, cancellationToken);
        }

        public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}