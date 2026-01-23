namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Consumers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.Interfaces.Consumers;
    using global::MassTransit;

    public abstract class BaseIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
        where TEvent : class, IIntegrationEvent
    {
        public async Task Consume(ConsumeContext<TEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            await this.HandleAsync(context.Message, CancellationToken.None);
        }

        public abstract Task HandleAsync(TEvent integrationMessage, CancellationToken cancellationToken);
    }
}