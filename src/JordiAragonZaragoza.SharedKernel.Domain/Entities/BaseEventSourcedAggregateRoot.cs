namespace JordiAragonZaragoza.SharedKernel.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public abstract class BaseEventSourcedAggregateRoot<TId> : BaseAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        where TId : class, IEntityId
    {
        protected BaseEventSourcedAggregateRoot(TId id)
            : base(id)
        {
        }

        protected BaseEventSourcedAggregateRoot()
        {
        }

        public void Load(IEnumerable<IDomainEvent> history)
        {
            ArgumentNullException.ThrowIfNull(history);
            this.Version = 0;

            foreach (var @event in history)
            {
                this.When(@event);
                this.Version++;
            }

            // Adjust the version to match the number of events applied. Starting from zero.
            if (this.Version > 0)
            {
                this.Version--;
            }
        }
    }
}