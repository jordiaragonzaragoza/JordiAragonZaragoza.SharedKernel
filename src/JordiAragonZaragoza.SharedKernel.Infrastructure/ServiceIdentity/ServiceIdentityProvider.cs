namespace JordiAragonZaragoza.SharedKernel.Infrastructure.ServiceIdentity
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;
    using Microsoft.Extensions.Options;

    public class ServiceIdentityProvider : IServiceIdentityProvider
    {
        private readonly string serviceName;

        public ServiceIdentityProvider(
            IOptions<ServiceIdentityOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.serviceName = options.Value.ServiceName ?? throw new ArgumentException("Service name must be provided.", nameof(options));
        }

        public string GetServiceName()
            => this.serviceName;
     }
}