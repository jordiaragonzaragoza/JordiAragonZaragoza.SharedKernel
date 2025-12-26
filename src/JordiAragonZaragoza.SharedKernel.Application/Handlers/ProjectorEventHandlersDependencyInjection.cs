namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class ProjectorEventHandlersDependencyInjection
    {
        public static IServiceCollection AddProjectorEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IEvent
            where THandler : class, IInMemoryEventHandler<TEvent>
        {
            return services.AddScoped<INotificationHandler<TEvent>, THandler>();
        }
    }
}