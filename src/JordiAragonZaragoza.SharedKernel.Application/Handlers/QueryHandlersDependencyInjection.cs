namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class QueryHandlersDependencyInjection
    {
        public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
            where TQuery : IQuery<TResponse>
            where THandler : class, IQueryHandler<TQuery, TResponse>
            where TResponse : notnull
        {
            return services.AddScoped<IRequestHandler<TQuery, Result<TResponse>>, THandler>();
        }
    }
}