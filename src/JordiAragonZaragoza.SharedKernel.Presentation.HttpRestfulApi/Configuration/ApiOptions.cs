namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using Asp.Versioning;

    /// <summary>
    /// Settings that vary from one API host to another and are known at compile time.
    /// Everything provider specific (authority, audience, Swagger login) comes from configuration.
    /// </summary>
    public sealed class ApiOptions
    {
        /// <summary>
        /// Gets the title shown in the Swagger documents.
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Gets the API version assumed when the request does not specify one.
        /// </summary>
        public ApiVersion DefaultApiVersion { get; init; } = new ApiVersion(2);

        /// <summary>
        /// Gets a value indicating whether CORS is enabled. Off by default: APIs that sit behind
        /// a gateway are called server to server and do not need it (browsers enforce CORS, not servers).
        /// </summary>
        public bool EnableCors { get; init; }
    }
}
