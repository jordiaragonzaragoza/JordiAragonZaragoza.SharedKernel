namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;

    public static class AnonymousRequestHelper
    {
        public static bool IsAnonymousAllowed(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var path = context.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var endpoint = context.GetEndpoint();

            if (endpoint is null)
            {
                return false;
            }

            var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>();

            return allowAnonymous is not null;
        }
    }
}