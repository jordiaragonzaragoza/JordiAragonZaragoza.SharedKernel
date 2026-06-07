namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.CatchUp
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Bindable settings for the all-stream catch-up subscription.
    /// These are read from configuration (e.g., appsettings.json) under
    /// the section defined by <see cref="Section"/>.
    /// Non-bindable options (FilterOptions, Credentials, etc.) are configured
    /// in code via <see cref="KurrentDbAllStreamSubscriptionOptions"/>.
    /// </summary>
    public class KurrentDbAllStreamSubscriptionSettings
    {
        public const string Section = "KurrentDb:AllStreamSubscription";
        public static readonly Guid DefaultSubscriptionId = new("cbbaeb7e-a087-44cc-75a0-08dc80991837");

        /// <summary>
        /// Gets or sets the unique identifier for this subscription.
        /// Defaults to a fixed GUID if not configured.
        /// </summary>
        [Required]
        public Guid SubscriptionId { get; set; } = DefaultSubscriptionId;

        /// <summary>
        /// Gets or sets a value indicating whether to resolve link events to their originals.
        /// </summary>
        public bool ResolveLinkTos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to silently skip events that fail deserialization
        /// instead of stopping the subscription.
        /// </summary>
        public bool IgnoreDeserializationErrors { get; set; }
    }
}