namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.HttpClientHelpers
{
    using System;
    using System.Web;

    public static class EndpointRouteHelpers
    {
        public static Uri BuildUriWithQueryParameters(string basePath, params (string Key, string Value)[] queryParams)
        {
            ArgumentNullException.ThrowIfNull(basePath, nameof(basePath));
            ArgumentNullException.ThrowIfNull(queryParams, nameof(queryParams));

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var (key, value) in queryParams)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                string placeholder = $"{{{key}}}";
                bool wasReplaced = basePath.Contains(placeholder, StringComparison.OrdinalIgnoreCase);

                basePath = basePath.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);

                if (!wasReplaced)
                {
                    query[key] = value;
                }
            }

            return new UriBuilder { Path = basePath, Query = query.ToString() }.Uri;
        }
    }
}