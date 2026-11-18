namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi
{
    using System;
    using System.Text.Json.Serialization;
    using Asp.Versioning;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class HttpRestfulApiDependencyInjection
    {
        /// <summary>
        /// Registers versioning, controllers, Swagger and (optionally) CORS.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="environment">The host environment.</param>
        /// <param name="apiOptions">The settings that vary per API host.</param>
        /// <returns>The service collection with the registered services.</returns>
        public static IServiceCollection AddSharedKernelPresentationHttpRestfulApi(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment,
            ApiOptions apiOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);
            ArgumentNullException.ThrowIfNull(apiOptions);

            services.AddSharedKernelJwtBearerAuthentication(configuration, environment);

            services.AddOptions<IdentityClaimsOptions>()
                .Bind(configuration.GetSection(IdentityClaimsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IUserIdentifierResolver, ClaimUserIdentifierResolver>();

            services.AddSingleton(apiOptions);
            services.AddSingleton(
                configuration.GetSection(SwaggerOAuth2Options.SectionName).Get<SwaggerOAuth2Options>()
                ?? new SwaggerOAuth2Options());

            services.AddAuthorization();

            if (apiOptions.EnableCors)
            {
                services.AddSharedKernelCors(configuration, environment);
            }

            services
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = apiOptions.DefaultApiVersion;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();

                    // How to read api version.
                    // More info: https://github.com/dotnet/aspnet-api-versioning/wiki/API-Version-Reader
                    /*options.ApiVersionReader = ApiVersionReader.Combine(
                         new UrlSegmentApiVersionReader(),
                         new QueryStringApiVersionReader("api-version"),
                         new HeaderApiVersionReader("X-Version"),
                         new MediaTypeApiVersionReader("version"));*/
                })
                .AddApiExplorer(setup =>
                {
                    // Add ApiExplorer to discover versions.
                    setup.GroupNameFormat = "'v'VVV";
                    setup.SubstituteApiVersionInUrl = true;
                });

            services
                .AddControllers(options =>
                {
                    options.AllowEmptyInputInBodyModelBinding = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                    // TODO: Review if there is some issue with System.Text.Json related with these settings
                    // https://github.com/dotnet/runtime/issues/1566
                    ////options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    ////options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => options.EnableAnnotations());
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}