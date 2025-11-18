namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Subscriptions
{
    using System;
    using KurrentDB.Client;

    using EventTypeFilter = global::KurrentDB.Client.EventTypeFilter;

    public class EventStoreDbSubscriptionToAllOptions
    {
        public Guid SubscriptionId { get; set; } = new Guid("cbbaeb7e-a087-44cc-75a0-08dc80991837"); // Use some random Guid as default.

        public SubscriptionFilterOptions FilterOptions { get; set; } =
            new(EventTypeFilter.ExcludeSystemEvents());

        public Action<KurrentDBClientOperationOptions>? ConfigureOperation { get; set; }

        public UserCredentials? Credentials { get; set; }

        public bool ResolveLinkTos { get; set; }

        public bool IgnoreDeserializationErrors { get; set; } = true;
    }
}