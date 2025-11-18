namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit
{
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Events;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters;
    using global::MassTransit;
    using global::MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    public static class MassTransitDependencyInjection
    {
        public static IServiceCollection AddMassTransitInternalBusRegistrations(this IServiceCollection services, string targetHostName)
        {
            services.AddOptions<MassTransitInternalBusOptions>()
                .BindConfiguration(MassTransitInternalBusOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.TryAddSingleton(new MassTransitInternalBusConsumersRegistrationConfigurator(_ => { }));

            services.AddMassTransit(busRegistrationConfigurator =>
            {
                busRegistrationConfigurator.SetKebabCaseEndpointNameFormatter();

                var serviceProvider = services.BuildServiceProvider();

                var internalBusOutboxConfigurator = serviceProvider.GetRequiredService<MassTransitInternalBusOutboxConfigurator>();
                internalBusOutboxConfigurator.Configure(busRegistrationConfigurator);

                // Run dynamic configuration from consumers.
                var internalBusConsumersRegistrationConfigurator = serviceProvider.GetRequiredService<MassTransitInternalBusConsumersRegistrationConfigurator>();
                internalBusConsumersRegistrationConfigurator.Configure(busRegistrationConfigurator);

                busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
                {
                    rabbitMqBusFactoryConfigurator.AutoStart = true;

                    rabbitMqBusFactoryConfigurator.Publish<INotification>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<IEvent>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<IDomainEvent>(p => p.Exclude = true);
                    rabbitMqBusFactoryConfigurator.Publish<BaseDomainEvent>(p => p.Exclude = true);

                    ConfigureFilters(rabbitMqBusFactoryConfigurator, busRegistrationContext);

                    var massTransitOptions = busRegistrationContext.GetRequiredService<IOptions<MassTransitInternalBusOptions>>().Value;

                    rabbitMqBusFactoryConfigurator.Host(massTransitOptions.RabbitMq.Url, h =>
                    {
                        h.ConnectionName(targetHostName);
                    });

                    rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
                });
            });

            return services;
        }

        private static void ConfigureFilters(
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
        }
    }
}