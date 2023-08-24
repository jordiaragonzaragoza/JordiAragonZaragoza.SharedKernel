﻿namespace JordiAragon.SharedKernel.Domain.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Ardalis.GuardClauses;
    using JordiAragon.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Domain.ValueObjects;

    public abstract class BaseAggregateRoot<TId, TIdType> : BaseAuditableEntity<TId>, IAggregateRoot<TId>, ISoftDelete
        where TId : BaseAggregateRootId<TIdType>
    {
        private readonly List<IDomainEvent> domainEvents = new();

        protected BaseAggregateRoot(TId id)
            : base(id)
        {
            this.Id = Guard.Against.Null(id, nameof(id));
        }

        // Required by EF.
        protected BaseAggregateRoot()
        {
        }

        public new BaseAggregateRootId<TIdType> Id { get; protected set; }

        [NotMapped]
        public IEnumerable<IDomainEvent> Events => this.domainEvents.AsReadOnly();

        public int Version { get; private set; } = -1;

        public bool IsDeleted { get; private set; }

        public void ClearEvents() => this.domainEvents.Clear();

        public void Load(IEnumerable<IDomainEvent> history)
        {
            foreach (var @event in history)
            {
                this.When(@event);
                this.Version++;
            }
        }

        protected void MarkAsDeleted()
        {
            this.IsDeleted = true;
        }

        protected void Apply(IDomainEvent domainEvent)
        {
            this.When(domainEvent);
            this.EnsureValidState();
            this.domainEvents.Add(domainEvent);
        }

        protected abstract void When(IDomainEvent domainEvent);

        protected abstract void EnsureValidState();
    }
}