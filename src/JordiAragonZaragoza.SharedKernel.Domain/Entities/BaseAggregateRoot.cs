namespace JordiAragonZaragoza.SharedKernel.Domain.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>, IAggregateRoot<TId>
       where TId : class, IEntityId
    {
        private readonly List<IDomainEvent> domainEvents = [];

        protected BaseAggregateRoot(TId id)
            : base(id)
        {
        }

        // Required by EF.
        protected BaseAggregateRoot()
        {
        }

        public uint Version { get; protected set; }

        [NotMapped]
        public IEnumerable<IDomainEvent> Events => this.domainEvents.AsReadOnly();

        public void ClearEvents()
        {
            this.domainEvents.Clear();
        }

        protected void Apply(IDomainEvent domainEvent)
        {
            this.When(domainEvent);
            this.EnsureValidState();
            this.domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// When is used to project events into the state of the aggregate.
        /// This means that it is responsible for applying the changes described in each event to the current state of the aggregate.
        /// </summary>
        /// <param name="domainEvent">The domain event to apply.</param>
        protected abstract void When(IDomainEvent domainEvent);

        /// <summary>
        /// This method checks that in any situation, the state of the entity is valid and if it is not, it will return an error result.
        /// When we call this method from any operation method, we can be sure that no matter what we try to do,
        /// our entity will always be in a valid state or the caller will get an error result.
        /// If your aggregate logic is completely deterministic, you might not need to implement EnsureValidState at all.
        /// The validation will be done in the Aggregate's public methods. That way, given a specific set of inputs or events,
        /// the aggregate will always reach the same state no matter when or in what context they are applied.
        /// The final state of the aggregate is predictable and repeatable. It does not depend on external factors or context.
        /// There are no dependencies on global variables, shared states, or external conditions that can alter the behavior of the event application methods.
        /// </summary>
        protected abstract void EnsureValidState();
    }
}