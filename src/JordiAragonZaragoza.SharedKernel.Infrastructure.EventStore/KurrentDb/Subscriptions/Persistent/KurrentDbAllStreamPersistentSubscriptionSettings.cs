namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Options for configuring the persistent subscription to the $all stream in KurrentDB.
    /// </summary>
    public class KurrentDbAllStreamPersistentSubscriptionSettings
    {
        /// <summary>
        /// The configuration section name for binding these options from configuration sources (e.g., appsettings.json).
        /// </summary>
        public const string Section = "KurrentDb:AllStreamPersistentSubscription";
        public const string DefaultGroupName = "all-stream-group";

        /// <summary>
        /// Gets or sets the consumer group name. Multiple instances using the same group name
        /// will coordinate their event consumption via checkpoints.
        /// Default: "all-stream-group".
        /// </summary>
        [Required]
        public string GroupName { get; set; } = DefaultGroupName;

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
    }
}
