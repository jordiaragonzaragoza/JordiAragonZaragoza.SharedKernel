namespace JordiAragonZaragoza.SharedKernel.Application.Commands
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public abstract class BaseCommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : notnull
    {
        private readonly List<IApplicationEvent> applicationEvents = [];

        public IEnumerable<IApplicationEvent> Events => this.applicationEvents.AsReadOnly();

        public abstract Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken);

        public void ClearEvents()
        {
            this.applicationEvents.Clear();
        }

        protected void RegisterApplicationEvent(IApplicationEvent applicationEvent)
        {
            this.applicationEvents.Add(applicationEvent);
        }
    }
}