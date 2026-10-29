namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpRestfulApiDependencyInjection
    {
        public static IServiceCollection AddSharedKernelPresentationHttpRestfulApi(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<IdentityClaimsOptions>()
                .Bind(configuration.GetSection(IdentityClaimsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IUserIdentifierResolver, ClaimUserIdentifierResolver>();

            return services;
        }
    }
}