namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers.Contracts.Interfaces
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces;
    using MassTransit;

    public interface IEventConsumer<in TEvent> : IConsumer<TEvent>
        where TEvent : class, IIntegrationEvent
    {
    }
}