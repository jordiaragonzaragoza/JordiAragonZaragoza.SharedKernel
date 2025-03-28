namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers.Contracts.Interfaces
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces;
    using MassTransit;

    public interface ICommandConsumer<in TCommand> : IConsumer<TCommand>
        where TCommand : class, IIntegrationCommand
    {
    }
}