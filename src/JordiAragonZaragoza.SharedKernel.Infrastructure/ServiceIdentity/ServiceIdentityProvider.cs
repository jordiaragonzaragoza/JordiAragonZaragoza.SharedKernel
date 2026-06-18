namespace JordiAragonZaragoza.SharedKernel.Infrastructure.ServiceIdentity
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Contracts;
    using Microsoft.Extensions.Options;

    public class ServiceIdentityProvider : IServiceIdentityProvider
    {
        private readonly string name;

        public ServiceIdentityProvider(
            IOptions<ServiceIdentityOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.name = options.Value.Name ?? throw new ArgumentException("Service name must be provided.", nameof(options));
        }

        public string GetName()
            => this.name;
     }
}