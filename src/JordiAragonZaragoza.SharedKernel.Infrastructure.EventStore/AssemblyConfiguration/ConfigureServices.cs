namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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

            return serviceCollection;
        }

#pragma warning disable CA2000 // Dispose objects before losing scope
        public static IServiceCollection AddSharedKernelInfrastructureKurrentDbAllStreamSubscription(
            this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<KurrentDbAllStreamSubscription>();
            serviceCollection.AddSingleton(new CancellationTokenSource());

            return serviceCollection.AddHostedService(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AllStreamSubscriptionBackgroundWorker>>();
                var kurrentDbAllStreamSubscription = serviceProvider.GetRequiredService<KurrentDbAllStreamSubscription>();
                var cancellationTokenSource = serviceProvider.GetRequiredService<CancellationTokenSource>();

                return new AllStreamSubscriptionBackgroundWorker(
                    logger,
                    cancellationToken =>
                        kurrentDbAllStreamSubscription.SubscribeToAllAsync(
                            new KurrentDbAllStreamSubscriptionOptions(),
                            cancellationTokenSource.Token));
            });
        }
#pragma warning restore CA2000 // Dispose objects before losing scope

/*services.AddSingleton<EventStoreDBSubscriptionsToAllCoordinator>();

            return services.AddKeyedSingleton<EventStoreDBSubscriptionToAll>(
                subscriptionOptions.SubscriptionId,
                (sp, _) =>
                {
                    var subscription = new EventStoreDBSubscriptionToAll(
                        sp.GetRequiredService<EventStoreClient>(),
                        sp.GetRequiredService<IServiceScopeFactory>(),
                        sp.GetRequiredService<ILogger<EventStoreDBSubscriptionToAll>>())
                        {
                            Options = subscriptionOptions, GetHandlers = handlers
                        };

                    return subscription;
                });*/

/*
#pragma warning disable CA2000 // Dispose objects before losing scope
        private static IServiceCollection AddEventStoreDB(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                        .AddSingleton(EventTypeMapper.Instance);
                        ////.AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)))
                        ////.AddTransient<EventStoreDbSubscriptionToAll, EventStoreDbSubscriptionToAll>()
                        ////.AddSingleton(new CancellationTokenSource());
        }
#pragma warning restore CA2000 // Dispose objects before losing scope
*/
        /*private static IServiceCollection AddEventStoreDBSubscriptionToAll(
            this IServiceCollection serviceCollection,
            EventStoreDbSubscriptionToAllOptions? subscriptionOptions = null)
        {
            return serviceCollection.AddHostedService(serviceProvider =>
            {
                var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                var logger = serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();
                var eventStoreDBSubscriptionToAll = serviceProvider.GetRequiredService<EventStoreDbSubscriptionToAll>();
                var cancellationTokenSource = serviceProvider.GetRequiredService<CancellationTokenSource>();

                return new BackgroundWorker(
                    logger,
                    cancellationToken =>
                        eventStoreDBSubscriptionToAll.SubscribeToAllAsync(
                            serviceScopeFactory,
                            subscriptionOptions ?? new EventStoreDbSubscriptionToAllOptions(),
                            cancellationTokenSource.Token));
            });
        }*/
    }
}