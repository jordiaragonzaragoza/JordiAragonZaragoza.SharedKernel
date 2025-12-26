namespace JordiAragonZaragoza.SharedKernel.Infrastructure
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Bus;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Cache;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Context.User;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.DateTime;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Identity;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.IdGenerator;
    using Microsoft.Extensions.DependencyInjection;

    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddSharedKernelInfrastructure(
            this IServiceCollection services)
        {
            services.AddSingleton<IDateTime, DateTimeService>();
            services.AddSingleton<IIdGenerator, IdGeneratorService>();
            services.AddSingleton<IPartitionContextService, PartitionContextService>();
            services.AddSingleton<IUserContextService, UserContextService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IIdentityService, IdentityService>();

            return services;
        }

        public static IServiceCollection AddSharedKernelInfrastructureCommandBus(
            this IServiceCollection services)
        {
            services.AddMediatRRegistrationsCommandBus();
            services.AddTransient<ICommandBus, CommandBus>();

            return services;
        }

        public static IServiceCollection AddSharedKernelInfrastructureQueryBus(
            this IServiceCollection services)
        {
            services.AddMediatRRegistrationsQueryBus();
            services.AddTransient<IQueryBus, QueryBus>();

            return services;
        }

        public static IServiceCollection AddSharedKernelInfrastructureProjections(
            this IServiceCollection services)
        {
            services.AddMediatRRegistrationsProjectionBus();
            services.AddTransient<IEventBus, InMemoryEventBus>();

            return services;
        }
    }
}