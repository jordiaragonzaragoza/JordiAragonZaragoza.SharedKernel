namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces
{
    using System;

    /// <summary>
    /// Integration Message is an event that is used to communicate with other systems outside the problem domain.
    /// The IntegrationMessage is a public event, part of the Published Language.
    /// </summary>
    public interface IIntegrationMessage
    {
        public Guid Id { get; }

        public string UserId { get; } // TODO: Add to metadata.

        public DateTimeOffset DateOccurredOnUtc { get; } // TODO: Add to metadata.

        public DateTimeOffset? DateDispatchedOnUtc { get; set; } // TODO: Add to metadata.
    }
}