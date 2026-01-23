namespace JordiAragonZaragoza.SharedKernel.Domain
{
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Events.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class DomainDependencyInjection
    {
        public static IServiceCollection AddSharedKernelDomain(this IServiceCollection services)
        {
            ////services.AddScoped<IEventsDispatcherService, EventsDispatcherService>();

            return services;
        }
    }
}