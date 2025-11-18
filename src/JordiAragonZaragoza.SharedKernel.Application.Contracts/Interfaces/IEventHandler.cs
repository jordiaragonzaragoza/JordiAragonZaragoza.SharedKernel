namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using MassTransit;

    public interface IEventHandler<in TEvent> : IBaseEventHandler<TEvent>, IConsumer<TEvent>
        where TEvent : class, IEvent
    {
    }
}