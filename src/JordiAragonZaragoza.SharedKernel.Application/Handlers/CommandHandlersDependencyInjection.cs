namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class CommandHandlersDependencyInjection
    {
        public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
            where TCommand : ICommand
            where THandler : class, ICommandHandler<TCommand>
        {
            return services.AddScoped<IRequestHandler<TCommand, Result>, THandler>();
        }

        public static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
            where TCommand : ICommand<TResponse>
            where THandler : class, ICommandHandler<TCommand, TResponse>
            where TResponse : notnull
        {
            return services.AddScoped<IRequestHandler<TCommand, Result<TResponse>>, THandler>();
        }
    }
}