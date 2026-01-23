namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Producers.MassTransit
{
    using global::MassTransit;

    /// <summary>
    /// This marker interface is required to implement MassTransit MultiBus.
    /// See <a href="https://masstransit.io/documentation/configuration/multibus">this link</a> for more information.
    /// </summary>
    public interface IIntegrationBus : IBus
    {
    }
}