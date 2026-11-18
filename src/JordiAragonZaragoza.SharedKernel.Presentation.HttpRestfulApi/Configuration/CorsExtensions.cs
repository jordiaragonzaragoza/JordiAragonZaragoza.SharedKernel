namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// CORS conventions. Reusable by any web host (APIs or a gateway).
    /// Origins come from the "Cors:AllowedOrigins" array in configuration, for example
    /// "Cors:AllowedOrigins:0" = "https://app.example.com" (env var: Cors__AllowedOrigins__0).
    /// </summary>
    public static class CorsExtensions
    {
        public const string PolicyName = "SharedKernelCorsPolicy";
        public const string AllowedOriginsKey = "Cors:AllowedOrigins";

        // Custom response headers a browser can only read if they are exposed.
        private static readonly string[] ExposedHeaders =
        [
            "x-correlation-id",
            "api-supported-versions",
            "api-deprecated-versions",
        ];

        public static IServiceCollection AddSharedKernelCors(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            var origins = (configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [])
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .ToArray();

            // Fail closed: outside Development an explicit allow list is mandatory.
            if (origins.Length == 0 && !environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    $"CORS is enabled but '{AllowedOriginsKey}' is not configured.");
            }

            services.AddCors(options => options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins);
                }
                else
                {
                    // Development only (guarded above): any frontend served from this machine.
                    policy.SetIsOriginAllowed(IsLoopbackOrigin);
                }

                // No AllowCredentials: tokens travel in the Authorization header, not in cookies.
                // Add it (with explicit origins) only if a cookie based BFF is introduced.
                policy.AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            }));

            return services;
        }

        public static IApplicationBuilder UseSharedKernelCors(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseCors(PolicyName);
        }

        private static bool IsLoopbackOrigin(string origin) =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback;
    }
}
