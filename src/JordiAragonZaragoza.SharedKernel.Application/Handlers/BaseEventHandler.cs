namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using MassTransit;

    public abstract class BaseEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        public async Task Consume(ConsumeContext<TEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            await this.HandleAsync(context.Message, CancellationToken.None);
        }

        public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}