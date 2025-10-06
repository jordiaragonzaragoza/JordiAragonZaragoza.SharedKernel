namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers.MassTransit
{
    using System;
    using global::MassTransit;

    public interface IMassTransitBusRegistrationConfigurator
    {
        Action<IBusRegistrationConfigurator> Configure { get; set; }
    }
}