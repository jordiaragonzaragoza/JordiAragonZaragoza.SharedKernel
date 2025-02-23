namespace JordiAragonZaragoza.SharedKernel.Application.Commands.Decorators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public class CommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> decorated;
        private readonly IEventsDispatcherService eventsDispatcherService;

        public CommandHandlerDecorator(
            IEventsDispatcherService eventsDispatcherService,
            ICommandHandler<TCommand> decorated)
        {
            this.eventsDispatcherService = Guard.Against.Null(eventsDispatcherService, nameof(eventsDispatcherService));
            this.decorated = Guard.Against.Null(decorated, nameof(decorated));
            this.Events = decorated.Events;
        }

        public IEnumerable<IApplicationEvent> Events { get; init; }

        public void ClearEvents()
        {
            this.decorated.ClearEvents();
        }

        public async Task<Result> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var result = await this.decorated.Handle(request, cancellationToken);

            if (this.Events.Any())
            {
                await this.eventsDispatcherService.DispatchEventsFromEventableEntitiesAsync(new List<IEventsContainer<IApplicationEvent>> { this }, cancellationToken);
            }

            return result;
        }
    }
}