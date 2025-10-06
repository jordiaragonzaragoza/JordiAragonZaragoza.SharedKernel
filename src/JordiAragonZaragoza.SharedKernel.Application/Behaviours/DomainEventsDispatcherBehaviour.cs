namespace JordiAragonZaragoza.SharedKernel.Application.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using MediatR;

    /// <summary>
    /// The deferred approach to raise and dispatch events
    /// Dispatch Domain Events collection in a unique atomic transaction.
    /// BEFORE committing data (EF SaveChanges) into the DB. This makes
    /// a single transaction including side effects from the domain event
    /// handlers that are using the same DbContext with Scope lifetime.
    /// Note: If an unhandled exception occurs, all the saved changes will be rolled back
    /// by the UnitOfWorkBehaviour. All the operations related to a domain event finish
    /// successfully or none of them do.
    /// See <a href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation#what-is-a-domain-event">this link</a> for more information.
    /// </summary>
    /// <typeparam name="TRequest">The command.</typeparam>
    /// <typeparam name="TResponse">The command response.</typeparam>
    public class DomainEventsDispatcherBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IBaseCommand
        where TResponse : IResult
    {
        private readonly IEventsDispatcherService domainEventsDispatcher;

        public DomainEventsDispatcherBehaviour(
            IEventsDispatcherService domainEventsDispatcher)
        {
            this.domainEventsDispatcher = domainEventsDispatcher;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            var response = await next(cancellationToken);

            await this.domainEventsDispatcher.DispatchEventsFromAggregatesStoreAsync(cancellationToken);

            return response;
        }
    }
}