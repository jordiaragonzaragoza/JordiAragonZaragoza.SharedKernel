namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Swagger UI login settings, bound from the "Swagger:OAuth2" configuration section.
    /// They are provider specific (Keycloak, Entra ID...), so they are configuration, not code.
    /// When the endpoints are missing, Swagger only offers the "paste the token" scheme.
    /// </summary>
    public sealed class SwaggerOAuth2Options
    {
        public const string SectionName = "Swagger:OAuth2";

        private static readonly string[] DefaultScopes = ["openid", "profile"];

        public Uri? AuthorizationUrl { get; set; }

        public Uri? TokenUrl { get; set; }

        public string ClientId { get; set; } = "swagger-ui";

        /// <summary>
        /// Gets the configured scopes. Empty by default on purpose: the configuration
        /// binder appends to non empty collections instead of replacing them.
        /// </summary>
        public Collection<string> Scopes { get; } = [];

        public bool IsConfigured =>
            this.AuthorizationUrl?.IsAbsoluteUri == true && this.TokenUrl?.IsAbsoluteUri == true;

        public IReadOnlyCollection<string> EffectiveScopes =>
            this.Scopes.Count > 0 ? this.Scopes : DefaultScopes;
    }
}