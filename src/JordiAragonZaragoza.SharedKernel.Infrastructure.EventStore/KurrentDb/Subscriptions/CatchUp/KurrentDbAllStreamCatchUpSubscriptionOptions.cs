namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp
{
    using System;
    using KurrentDB.Client;

    using EventTypeFilter = global::KurrentDB.Client.EventTypeFilter;

    /// <summary>
    /// Full runtime options for the all-stream catch-up subscription.
    /// Non-bindable properties (FilterOptions, Credentials, ConfigureOperation)
    /// are configured in code. Bindable properties come from
    /// <see cref="KurrentDbAllStreamCatchUpSubscriptionSettings"/>.
    /// </summary>
    public class KurrentDbAllStreamCatchUpSubscriptionOptions
    {
        public Guid SubscriptionId { get; set; } = KurrentDbAllStreamCatchUpSubscriptionSettings.DefaultSubscriptionId;

        public SubscriptionFilterOptions FilterOptions { get; set; } =
            new(EventTypeFilter.ExcludeSystemEvents());

        public Action<KurrentDBClientOperationOptions>? ConfigureOperation { get; set; }

        public UserCredentials? Credentials { get; set; }

        public bool ResolveLinkTos { get; set; }

        public bool IgnoreDeserializationErrors { get; set; }

        /// <summary>
        /// Applies bindable settings on top of the current options,
        /// overriding only the properties that settings exposes.
        /// </summary>
        /// <param name="settings">The bindable settings to apply.</param>
        /// <returns>The updated options instance for chaining.</returns>
        public KurrentDbAllStreamCatchUpSubscriptionOptions ApplySettings(
            KurrentDbAllStreamCatchUpSubscriptionSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            this.SubscriptionId = settings.SubscriptionId;
            this.ResolveLinkTos = settings.ResolveLinkTos;
            this.IgnoreDeserializationErrors = settings.IgnoreDeserializationErrors;

            return this;
        }
    }
}