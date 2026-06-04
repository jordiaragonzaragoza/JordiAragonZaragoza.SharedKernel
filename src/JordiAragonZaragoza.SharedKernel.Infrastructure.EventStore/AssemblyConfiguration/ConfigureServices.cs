namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.AssemblyConfiguration
{
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent;
    using KurrentDB.Client;
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

            // KurrentDBPersistentSubscriptionsClient se deriva del KurrentDBClient registrado por Aspire,
            // copiando su configuración (conexión, credenciales, interceptors, etc.).
            // Registramos como singleton porque el cliente gRPC subyacente es thread-safe y caro de construir.
            serviceCollection.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<KurrentDBClient>();
                var settings = client.GetSettings().Copy();
                return new KurrentDBPersistentSubscriptionsClient(settings);
            });

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

        public static IServiceCollection AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription(
            this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<KurrentDbAllStreamPersistentSubscription>();

            return serviceCollection.AddHostedService(serviceProvider =>
            {
                var subscription = serviceProvider.GetRequiredService<KurrentDbAllStreamPersistentSubscription>();
                var logger = serviceProvider.GetRequiredService<ILogger<AllStreamPersistentSubscriptionBackgroundWorker>>();
                var options = new KurrentDbAllStreamPersistentSubscriptionOptions();

                return new AllStreamPersistentSubscriptionBackgroundWorker(
                    subscription,
                    options,
                    logger);
            });
        }
    }
}