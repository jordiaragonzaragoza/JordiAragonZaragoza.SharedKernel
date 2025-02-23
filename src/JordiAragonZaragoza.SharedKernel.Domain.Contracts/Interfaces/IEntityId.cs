﻿namespace JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces
{
    /// <summary>
    /// Generic abstraction for a entity id.
    /// </summary>
    /// <typeparam name="TIdType">The value for the entity id.</typeparam>
    public interface IEntityId<out TIdType> : IEntityId
        where TIdType : notnull
    {
        TIdType Value { get; }
    }
}