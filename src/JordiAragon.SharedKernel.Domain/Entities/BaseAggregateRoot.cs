﻿namespace JordiAragon.SharedKernel.Domain.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Ardalis.GuardClauses;
    using JordiAragon.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Domain.ValueObjects;

    public abstract class BaseAggregateRoot<TId, TIdType> : BaseEntity<TId>, IAggregateRoot<TId>
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

        public void ClearEvents() => this.domainEvents.Clear();

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