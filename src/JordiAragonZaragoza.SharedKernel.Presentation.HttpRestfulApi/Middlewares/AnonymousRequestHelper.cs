namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    public static class AnonymousRequestHelper
    {
        // Paths that correspond to pure infrastructure.
        // UseSwaggerUI is a classic middleware — it has no endpoint metadata,
        // so detection must be done by path prefix.
        private static readonly string[] InfrastructurePrefixes = new[]
        {
            "/swagger",
            "/health",
            "/metrics",
            "/ready",
            "/alive",
            "/favicon.ico",
        };

        /// <summary>
        /// True for pure infrastructure paths (swagger, health, metrics).
        /// These are either classic middlewares without endpoint metadata
        /// or endpoints explicitly marked with InfrastructureEndpointAttribute.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>True if the request is for an infrastructure endpoint; otherwise, false.</returns>
        public static bool IsInfrastructureEndpoint(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var path = context.Request.Path;

            if (InfrastructurePrefixes.Any(prefix => path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return context.GetEndpoint()
                ?.Metadata
                .GetMetadata<InfrastructureEndpointAttribute>() is not null;
        }
    }
}