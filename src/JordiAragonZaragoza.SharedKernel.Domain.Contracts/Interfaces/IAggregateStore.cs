namespace JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces
{
    using System.Collections.Generic;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    public interface IAggregateStore
    {
        IEnumerable<IEventsContainer<IEvent>> EventableEntities { get; }
    }
}