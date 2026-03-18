namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.HttpClientHelpers
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public static class HttpClientGetExtensionMethods
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static async Task<T> GetAndDeserializeAsync<T>(this HttpClient client, string requestUri, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(requestUri);

            var uri = new Uri(requestUri, UriKind.RelativeOrAbsolute);
            return await GetAndDeserializeInternalAsync<T>(client, uri, cancellationToken);
        }

        public static async Task<T> GetAndDeserializeAsync<T>(this HttpClient client, Uri requestUri, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(requestUri);

            return await GetAndDeserializeInternalAsync<T>(client, requestUri, cancellationToken);
        }

        private static async Task<T> GetAndDeserializeInternalAsync<T>(HttpClient client, Uri requestUri, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);

            response.EnsureSuccessStatusCode();

            string text = await response.Content.ReadAsStringAsync(cancellationToken);
            T? deserialized = JsonSerializer.Deserialize<T>(text, DefaultJsonOptions);

            return deserialized ?? throw new InvalidOperationException(
                $"Failed to deserialize response as '{typeof(T).Name}'. The response was null or invalid.");
        }
    }
}