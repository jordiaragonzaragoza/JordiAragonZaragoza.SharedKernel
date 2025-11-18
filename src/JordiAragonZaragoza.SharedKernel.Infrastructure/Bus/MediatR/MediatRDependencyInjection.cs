namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR
{
    using global::MediatR;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours;
    using Microsoft.Extensions.DependencyInjection;

    public static class MediatRDependencyInjection
    {
        public static IServiceCollection AddMediatRRegistrationsCommandBus(
            this IServiceCollection services)
        {
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(MediatRDependencyInjection).Assembly);

                // Configure MediatR Pipeline
                options.AddOpenRequestPreProcessor(typeof(LoggerBehaviour<>));
                options.AddOpenBehavior(typeof(ExceptionHandlerPipelineBehaviour<,>));
                options.AddOpenBehavior(typeof(UnitOfWorkBehaviour<,>));
                options.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
                options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                options.AddOpenBehavior(typeof(CachingBehavior<,>));
                options.AddOpenBehavior(typeof(InvalidateCachingBehavior<,>));
                options.AddOpenBehavior(typeof(PerformancePipelineBehaviour<,>));
            });

            services.AddScoped<IMediator, Mediator>();

            return services;
        }

        public static IServiceCollection AddMediatRRegistrationsQueryBus(
            this IServiceCollection services)
        {
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(MediatRDependencyInjection).Assembly);

                // Configure MediatR Pipeline
                options.AddOpenRequestPreProcessor(typeof(LoggerBehaviour<>));
                options.AddOpenBehavior(typeof(ExceptionHandlerPipelineBehaviour<,>));
                options.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
                options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                options.AddOpenBehavior(typeof(CachingBehavior<,>));
                options.AddOpenBehavior(typeof(InvalidateCachingBehavior<,>));
                options.AddOpenBehavior(typeof(PerformancePipelineBehaviour<,>));
            });

            services.AddScoped<IMediator, Mediator>();

            return services;
        }
    }
}