namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages.Interfaces
{
    /// <summary>
    /// Represents an integration command that can be sent through a messaging system.
    /// Commands express intent and are typically processed by a single recipient.
    /// <para>
    /// ⚠️ Warning: A bounded context should not send commands to another bounded context
    /// to enforce state changes. Doing so introduces strong coupling and violates the principle
    /// of autonomy in distributed architectures. Instead, consider using integration events
    /// to notify other contexts about relevant changes without enforcing direct modifications.
    /// </para>
    /// </summary>
    public interface IIntegrationCommand : IIntegrationMessage
    {
    }
}