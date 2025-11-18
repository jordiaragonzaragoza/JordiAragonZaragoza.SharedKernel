namespace JordiAragonZaragoza.SharedKernel.Domain.Entities
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Domain.ValueObjects;

    public abstract class BaseAggregateRoot<TId, TIdType> : BaseAggregateRoot<TId>
        where TId : BaseAggregateRootId<TIdType>
        where TIdType : notnull
    {
        protected BaseAggregateRoot(TId id)
            : base(id)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        // Required by EF.
        protected BaseAggregateRoot()
        {
        }
    }
}