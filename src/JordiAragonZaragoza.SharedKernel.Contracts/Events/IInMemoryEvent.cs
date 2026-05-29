namespace JordiAragonZaragoza.SharedKernel.Contracts.Events
{
    using System;
    using MediatR;

    /// <summary>
    /// The In Memory Event is an event that occurs within the problem (living inside a bounded context)
    /// and is used to communicate a change in the state of the aggregate inside from source transaction.
    /// This is a private event and part of Ubiquitous Language.
    /// </summary>
    public interface IInMemoryEvent : IEvent
    {
        public bool IsPublished { get; set; }
    }
}