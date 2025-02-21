namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionHelper
    {
        public static IServiceCollection Remove<TService>(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            var serviceDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(TService));

            if (serviceDescriptor != null)
            {
                _ = services.Remove(serviceDescriptor);
            }

            return services;
        }
    }
}