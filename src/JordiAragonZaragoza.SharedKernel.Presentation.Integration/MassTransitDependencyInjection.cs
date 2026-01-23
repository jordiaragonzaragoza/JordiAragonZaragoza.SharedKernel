namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration
{
    using JordiAragonZaragoza.SharedKernel.Presentation.Integration.Consumers.MassTransit;
    using JordiAragonZaragoza.SharedKernel.Presentation.Integration.Producers.MassTransit;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    public static class MassTransitDependencyInjection
    {
        public static IServiceCollection AddSharedKernelPresentationIntegrationBusRegistrations(this IServiceCollection services, string targetHostName)
        {
            services.AddOptions<MassTransitIntegrationBusOptions>()
                .BindConfiguration(MassTransitIntegrationBusOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.TryAddSingleton(new MassTransitIntegrationBusConsumersRegistrationConfigurator(_ => { }));

            services.AddMassTransit<IIntegrationBus>(busRegistrationConfigurator =>
            {
                busRegistrationConfigurator.SetKebabCaseEndpointNameFormatter();

                var serviceProvider = services.BuildServiceProvider();

                // Run dynamic configuration from consumers.
                var internalBusConsumersRegistrationConfigurator = serviceProvider.GetRequiredService<MassTransitIntegrationBusConsumersRegistrationConfigurator>();
                internalBusConsumersRegistrationConfigurator.Configure(busRegistrationConfigurator);

                busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
                {
                    rabbitMqBusFactoryConfigurator.AutoStart = true;

                    /*rabbitMqBusFactoryConfigurator.Publish<INotification>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<IEvent>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<IDomainEvent>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<BaseDomainEvent>(p => p.Exclude = true);*/

                    ////ConfigureFilters(rabbitMqBusFactoryConfigurator, busRegistrationContext);

                    var massTransitOptions = busRegistrationContext.GetRequiredService<IOptions<MassTransitIntegrationBusOptions>>().Value;

                    rabbitMqBusFactoryConfigurator.Host(massTransitOptions.RabbitMq.Url, h =>
                    {
                        h.ConnectionName(targetHostName);
                    });

                    rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
                });
            });

            return services;
        }

        /*private static void ConfigureFilters(
            IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
            IBusRegistrationContext busRegistrationContext)
        {
            // Consumer Filters
            rabbitMqBusFactoryConfigurator.UseConsumeFilter(typeof(PartitionContextConsumerFilter<>), busRegistrationContext);
            rabbitMqBusFactoryConfigurator.UseConsumeFilter(typeof(UserContextConsumerFilter<>), busRegistrationContext);
            rabbitMqBusFactoryConfigurator.UseConsumeFilter(typeof(GlobalExceptionConsumerFilter<>), busRegistrationContext);

            // Producers Filters
            rabbitMqBusFactoryConfigurator.UseSendFilter(typeof(PartitionContextProducerFilter<>), busRegistrationContext);
            rabbitMqBusFactoryConfigurator.UsePublishFilter(typeof(PartitionContextProducerFilter<>), busRegistrationContext);
            rabbitMqBusFactoryConfigurator.UseSendFilter(typeof(UserContextProducerFilter<>), busRegistrationContext);
            rabbitMqBusFactoryConfigurator.UsePublishFilter(typeof(UserContextProducerFilter<>), busRegistrationContext);
        }*/
    }
}