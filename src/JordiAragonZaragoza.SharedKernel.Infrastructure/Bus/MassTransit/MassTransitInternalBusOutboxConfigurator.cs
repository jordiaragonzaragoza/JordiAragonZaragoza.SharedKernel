namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit
{
    using System;
    using global::MassTransit;

    public class MassTransitInternalBusOutboxConfigurator
    {
        private readonly Action<IBusRegistrationConfigurator> configure;

        public MassTransitInternalBusOutboxConfigurator(Action<IBusRegistrationConfigurator> configure)
        {
            this.configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        public void Configure(IBusRegistrationConfigurator configurator)
        {
            this.configure(configurator);
        }
    }
}