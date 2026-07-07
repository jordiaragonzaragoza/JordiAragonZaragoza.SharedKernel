namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent
{
    using System;
    using KurrentDB.Client;

    /// <summary>
    /// Options for configuring the persistent subscription to the $all stream in KurrentDB.
    /// Non-bindable properties (FilterOptions, Credentials, ConfigureOperation)
    /// are configured in code. Bindable properties come from
    /// <see cref="KurrentDbAllStreamPersistentSubscriptionSettings"/>.
    /// </summary>
    public class KurrentDbAllStreamPersistentSubscriptionOptions
    {
        /// <summary>
        /// Gets or sets the consumer group name. Multiple instances using the same group name
        /// will coordinate their event consumption via checkpoints.
        /// Default: "all-stream-group".
        /// </summary>
        public string GroupName { get; set; } = KurrentDbAllStreamPersistentSubscriptionSettings.DefaultGroupName;

        /// <summary>
        /// Gets or sets the filter options for filtering events on the server side before transmission.
        /// </summary>
        public SubscriptionFilterOptions? FilterOptions { get; set; } =
            new(EventTypeFilter.ExcludeSystemEvents());

        /// <summary>
        /// Gets or sets the credentials to use when connecting to KurrentDB.
        /// </summary>
        public UserCredentials? Credentials { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to resolve link events to their target event data.
        /// </summary>
        public bool ResolveLinkTos { get; set; }

        /// <summary>
        /// Gets or sets the buffer size for the persistent subscription channel.
        /// Determines how many messages can be buffered before being delivered to the subscriber.
        /// Default: 10.
        /// </summary>
        public int BufferSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore deserialization errors and continue processing.
        /// </summary>
        public bool IgnoreDeserializationErrors { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the server will retry delivering a failed event
        /// before automatically parking it to the dead-letter stream
        /// ($persistentsubscription-{groupName}-parked).
        /// Parking is handled natively by KurrentDB; no client-side counting is required.
        /// Default: 5.
        /// </summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>
        /// Applies bindable settings on top of the current options,
        /// overriding only the properties that settings exposes.
        /// </summary>
        /// <param name="settings">The bindable settings to apply.</param>
        /// <returns>The updated options instance for chaining.</returns>
        public KurrentDbAllStreamPersistentSubscriptionOptions ApplySettings(
            KurrentDbAllStreamPersistentSubscriptionSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            this.GroupName = settings.GroupName;
            this.ResolveLinkTos = settings.ResolveLinkTos;
            this.IgnoreDeserializationErrors = settings.IgnoreDeserializationErrors;
            this.MaxRetryCount = settings.MaxRetryCount;

            return this;
        }
    }
}
