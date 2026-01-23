namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using MediatR;

    public interface IInMemoryEventHandler<in TEvent> : IBaseEventHandler<TEvent>, INotificationHandler<TEvent>
        where TEvent : IEvent
    {
    }
}