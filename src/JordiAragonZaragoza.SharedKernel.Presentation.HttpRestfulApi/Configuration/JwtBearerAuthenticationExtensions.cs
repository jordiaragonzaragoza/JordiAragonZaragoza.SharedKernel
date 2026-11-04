namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Helpers;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Provider agnostic JWT bearer authentication. Keycloak and Entra ID are both standard OIDC
    /// providers, so what changes between environments is configuration, not code:
    /// <list type="bullet">
    /// <item><description>Authentication:Schemes:Bearer:Authority</description></item>
    /// <item><description>Authentication:Schemes:Bearer:Audience</description></item>
    /// <item><description>Authentication:Claims:UserIdClaimType ("sub" for Keycloak, "oid" for Entra ID)</description></item>
    /// </list>
    /// </summary>
    public static class JwtBearerAuthenticationExtensions
    {
        private const string SectionName = "Authentication:Schemes:Bearer";

        public static IServiceCollection AddSharedKernelJwtBearerAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            var section = configuration.GetSection(SectionName);

            services.AddAuthentication()
                    .AddJwtBearer(options =>
                    {
                        options.Authority = section["Authority"];
                        options.Audience = section["Audience"];

                        // Keep the original claim names (sub, oid, preferred_username...).
                        options.MapInboundClaims = false;

                        // Plain http is only acceptable for the local Keycloak container.
                        options.RequireHttpsMetadata =
                            section.GetValue<bool?>("RequireHttpsMetadata") ?? !environment.IsDevelopment();
                    });

            // Fail at startup, not on the first request, when a deployed environment is misconfigured.
            // Skipped in Development so functional tests (fake auth handler) need no authority.
            if (!environment.IsDevelopment())
            {
                services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                        .Validate(
                            options => OptionsHelper.IsConfigured(options.Authority) && OptionsHelper.IsConfigured(options.Audience),
                            $"'{SectionName}:Authority' and '{SectionName}:Audience' must be configured.")
                        .ValidateOnStart();
            }

            return services;
        }
    }
}
