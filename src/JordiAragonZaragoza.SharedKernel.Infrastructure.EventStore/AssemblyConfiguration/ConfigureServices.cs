namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Subscriptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class ConfigureServices
    {
        public static IServiceCollection AddSharedKernelInfrastructureEventStoreDbBusiness(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddScoped<IEventStore, EventStoreDbEventStore>();
            serviceCollection.AddScoped<IAggregateStore, EventStoreDbEventStore>();
            serviceCollection.AddScoped<IUnitOfWork, EventStoreDbEventStore>();

            _ = serviceCollection
                .AddEventStoreDB();
                ////AddEventStoreDBSubscriptionToAll();

            return serviceCollection;
        }

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