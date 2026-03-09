namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.HttpClientHelpers
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public static class HttpClientPutExtensionMethods
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static async Task<T> PutAndDeserializeAsync<T>(this HttpClient client, string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(requestUri);

            var uri = new Uri(requestUri, UriKind.RelativeOrAbsolute);
            return await PutAndDeserializeInternalAsync<T>(client, uri, content, cancellationToken);
        }

        public static async Task<T> PutAndDeserializeAsync<T>(this HttpClient client, Uri requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(requestUri);

            return await PutAndDeserializeInternalAsync<T>(client, requestUri, content, cancellationToken);
        }

        private static async Task<T> PutAndDeserializeInternalAsync<T>(this HttpClient client, Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            using var response = await client.PutAsync(requestUri, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            string text = await response.Content.ReadAsStringAsync(cancellationToken);
            T? deserialized = JsonSerializer.Deserialize<T>(text, DefaultJsonOptions);

            return deserialized ?? throw new InvalidOperationException(
                $"Failed to deserialize response as '{typeof(T).Name}'. The response was null or invalid.");
        }
    }
}