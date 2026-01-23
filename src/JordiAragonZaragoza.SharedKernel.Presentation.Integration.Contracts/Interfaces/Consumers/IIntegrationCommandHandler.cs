namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.Interfaces.Consumers
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using MassTransit;

    /// <summary>
    /// Defines a consumer for handling integration commands within a messaging system.
    /// <para>
    /// ⚠️ Warning: A bounded context should not consume commands from another bounded context
    /// to enforce state changes. Doing so introduces strong coupling and violates the principle
    /// of autonomy in distributed architectures. Instead, consider using integration events
    /// to react to state changes without enforcing direct modifications.
    /// </para>
    /// </summary>
    /// <typeparam name="TCommand">The type of the integration command to be consumed.</typeparam>
    public interface IIntegrationCommandHandler<in TCommand> : IBaseIntegrationMessageHandler<TCommand>, IConsumer<TCommand>
        where TCommand : class, IIntegrationCommand
    {
    }
}