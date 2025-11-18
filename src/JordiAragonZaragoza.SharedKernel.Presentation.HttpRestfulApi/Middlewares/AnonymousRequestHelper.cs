namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using Microsoft.AspNetCore.Http;

    public static class AnonymousRequestHelper
    {
        public static bool IsAnonymousAllowed(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var path = context.Request.Path.Value ?? string.Empty;

            // Infrastructure (no authentication and no partition context)
            if (path.StartsWith("/swagger", StringComparison.InvariantCulture) ||
                path.StartsWith("/health", StringComparison.InvariantCulture) ||
                path.StartsWith("/metrics", StringComparison.InvariantCulture))
            {
                return true;
            }

            // TODO: REMOVE! TEMPORAL TILL AUTH IS IMPLEMENTED
            if (path.Equals("/api/payment193expenses/create", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/api/payment193expenses/update", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/api/payment193expenses/delete", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/api/exampleReactions", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}