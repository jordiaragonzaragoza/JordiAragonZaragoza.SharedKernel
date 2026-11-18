namespace JordiAragonZaragoza.SharedKernel.Hosting.Configuration
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddHostConfigurations(
            this IHostApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddSerilog(loggerConfiguration =>
                loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

            // Configure Dependency Injection validation.
            builder.ConfigureContainer(
                new DefaultServiceProviderFactory(
                    new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true,
                    }));

            return builder;
        }
    }
}