namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent;
    using KurrentDB.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public static class ConfigureServices
    {
        public static IServiceCollection AddSharedKernelInfrastructureKurrentDbBusiness(
            this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<KurrentDbEventStore>();
            serviceCollection.AddScoped<IEventStore>(sp => sp.GetRequiredService<KurrentDbEventStore>());
            serviceCollection.AddScoped<IAggregateStore>(sp => sp.GetRequiredService<KurrentDbEventStore>());
            serviceCollection.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<KurrentDbEventStore>());
            serviceCollection.AddSingleton(EventTypeMapper.Instance);

            // KurrentDBPersistentSubscriptionsClient derives from the KurrentDBClient registered by Aspire,
            // copying its configuration (connection, credentials, interceptors, etc.).
            // We register it as a singleton because the underlying gRPC client is thread-safe and expensive to construct.
            serviceCollection.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<KurrentDBClient>();
                var settings = client.GetSettings().Copy();
                return new KurrentDBPersistentSubscriptionsClient(settings);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddSharedKernelInfrastructureKurrentDbAllStreamSubscription(
            this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddOptions<KurrentDbAllStreamSubscriptionSettings>()
                .BindConfiguration(KurrentDbAllStreamSubscriptionSettings.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            serviceCollection.AddSingleton<KurrentDbAllStreamSubscription>();

            return serviceCollection.AddHostedService(serviceProvider =>
            {
                var kurrentDbAllStreamSubscription =
                    serviceProvider.GetRequiredService<KurrentDbAllStreamSubscription>();

                var logger =
                    serviceProvider.GetRequiredService<ILogger<AllStreamSubscriptionBackgroundWorker>>();

                // Read bindable settings (from appsettings or defaults).
                var settings =
                    serviceProvider.GetRequiredService<IOptions<KurrentDbAllStreamSubscriptionSettings>>().Value;

                // Build full runtime options starting from defaults, then apply settings.
                // Non-bindable properties (FilterOptions, Credentials, etc.) keep their defaults
                // or can be overridden here in code before calling ApplySettings.
                var options = new KurrentDbAllStreamSubscriptionOptions()
                    .ApplySettings(settings);

                return new AllStreamSubscriptionBackgroundWorker(
                    logger,
                    cancellationToken =>
                        kurrentDbAllStreamSubscription.SubscribeToAllAsync(
                            options,
                            cancellationToken));
            });
        }

        public static IServiceCollection AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription(
            this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddOptions<KurrentDbAllStreamPersistentSubscriptionSettings>()
                .BindConfiguration(KurrentDbAllStreamPersistentSubscriptionSettings.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            serviceCollection.AddTransient<KurrentDbAllStreamPersistentSubscription>();

            return serviceCollection.AddHostedService(serviceProvider =>
            {
                var subscription =
                    serviceProvider.GetRequiredService<KurrentDbAllStreamPersistentSubscription>();

                var logger =
                    serviceProvider.GetRequiredService<ILogger<AllStreamPersistentSubscriptionBackgroundWorker>>();

                var settings =
                    serviceProvider.GetRequiredService<IOptions<KurrentDbAllStreamPersistentSubscriptionSettings>>().Value;

                var options = new KurrentDbAllStreamPersistentSubscriptionOptions()
                    .ApplySettings(settings);

                return new AllStreamPersistentSubscriptionBackgroundWorker(
                    subscription,
                    options,
                    logger);
            });
        }
    }
}