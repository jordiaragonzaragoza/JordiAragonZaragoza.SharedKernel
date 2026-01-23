namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Consumers.MassTransit
{
    using System;
    using global::MassTransit;

    public class MassTransitIntegrationBusConsumersRegistrationConfigurator
    {
        private readonly Action<IBusRegistrationConfigurator> configure;

        public MassTransitIntegrationBusConsumersRegistrationConfigurator(Action<IBusRegistrationConfigurator> configure)
        {
            this.configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        /// <summary>
        /// Applies custom MassTransit bus configuration, such as registering consumers,
        /// sagas, or activities. This method delegates the configuration logic defined
        /// at the host level to avoid get same configuration to all host when not needed.
        /// </summary>
        /// <param name="configurator">The <see cref="IBusRegistrationConfigurator"/> instance used to configure MassTransit.</param>
        public void Configure(IBusRegistrationConfigurator configurator)
        {
            this.configure(configurator);
        }
    }
}