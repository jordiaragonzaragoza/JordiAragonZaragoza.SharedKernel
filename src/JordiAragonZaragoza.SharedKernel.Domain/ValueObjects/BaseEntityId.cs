namespace JordiAragonZaragoza.SharedKernel.Domain.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    public abstract class BaseEntityId<TIdType> : BaseValueObject, IEntityId<TIdType>
        where TIdType : notnull
    {
        protected BaseEntityId(TIdType value)
        {
            _ = Guard.Against.Default(value, nameof(value));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            this.Value = value;
        }

        // Required by EF.
        protected BaseEntityId()
        {
        }

        public TIdType Value { get; init; } = default!;

        public static implicit operator TIdType(BaseEntityId<TIdType> self)
        {
            ArgumentNullException.ThrowIfNull(self, nameof(self));

            return self.Value;
        }

        public TIdType FromBaseEntityId(BaseEntityId<TIdType> self)
        {
            return self;
        }

        public override string? ToString()
        {
            return this.Value.ToString();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}