namespace JordiAragonZaragoza.SharedKernel.Application
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Application.Pipelines;
    using Microsoft.Extensions.DependencyInjection;

    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddSharedKernelApplicationCommandBus(this IServiceCollection services)
        {
            services.AddTransient<IRequestLoggerService, RequestLoggerService>();
            services.AddTransient<IRequestExceptionHandlerService, RequestExceptionHandlerService>();
            services.AddTransient<IRequestUnitOfWorkService, RequestUnitOfWorkService>();
            services.AddTransient(typeof(IRequestAuthorizationService<,>), typeof(RequestAuthorizationService<,>));
            services.AddTransient(typeof(IRequestValidationService<,>), typeof(RequestValidationService<,>));
            services.AddTransient<IRequestCachingService, RequestCachingService>();
            services.AddTransient<IRequestInvalidateCachingService, RequestInvalidateCachingService>();
            services.AddTransient<IRequestPerformanceTrackingService, RequestPerformanceTrackingService>();

            return services;
        }

        public static IServiceCollection AddSharedKernelApplicationQueryBus(this IServiceCollection services)
        {
            services.AddTransient<IRequestLoggerService, RequestLoggerService>();
            services.AddTransient<IRequestExceptionHandlerService, RequestExceptionHandlerService>();
            services.AddTransient(typeof(IRequestAuthorizationService<,>), typeof(RequestAuthorizationService<,>));
            services.AddTransient(typeof(IRequestValidationService<,>), typeof(RequestValidationService<,>));
            services.AddTransient<IRequestCachingService, RequestCachingService>();
            services.AddTransient<IRequestInvalidateCachingService, RequestInvalidateCachingService>();
            services.AddTransient<IRequestPerformanceTrackingService, RequestPerformanceTrackingService>();

            return services;
        }

        public static IServiceCollection AddSharedKernelApplicationProjectionsEventBus(this IServiceCollection services)
        {
            // TODO: Complete with projections event bus crosscutting services.
            return services;
        }
    }
}