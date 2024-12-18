﻿namespace JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;

    public interface IEventsDispatcherService
    {
        Task DispatchEventsFromAggregatesStoreAsync(CancellationToken cancellationToken = default);

        Task DispatchEventsFromEventableEntitiesAsync(IEnumerable<IEventsContainer<IEvent>> eventableEntities, CancellationToken cancellationToken = default);
    }
}