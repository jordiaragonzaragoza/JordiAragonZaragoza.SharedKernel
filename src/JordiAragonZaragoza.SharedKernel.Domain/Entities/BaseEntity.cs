namespace JordiAragonZaragoza.SharedKernel.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Exceptions;

    public abstract class BaseEntity<TId> : IEqualityComparer<BaseEntity<TId>>, IEntity<TId>, IIgnoreDependency
        where TId : class, IEntityId
    {
        protected BaseEntity(TId id)
        {
            this.Id = Guard.Against.Null(id, nameof(id));
        }

        // Required by EF
        protected BaseEntity()
        {
        }

        public TId Id { get; protected set; } = default!;

        public bool Equals(BaseEntity<TId>? x, BaseEntity<TId>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(BaseEntity<TId> obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.Id.GetHashCode();
        }

        protected static void CheckRule(IBusinessRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            if (rule.IsBroken())
            {
                throw new BusinessRuleValidationException(rule);
            }
        }
    }
}