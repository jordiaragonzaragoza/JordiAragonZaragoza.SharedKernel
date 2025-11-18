namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.Interfaces.Consumers
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using MassTransit;

    public interface IIntegrationEventHandler<in TEvent> : IBaseIntegrationMessageHandler<TEvent>, IConsumer<TEvent>
        where TEvent : class, IIntegrationEvent
    {
    }
}