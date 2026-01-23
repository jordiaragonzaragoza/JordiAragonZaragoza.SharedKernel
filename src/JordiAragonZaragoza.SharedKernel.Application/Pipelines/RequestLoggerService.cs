namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class RequestLoggerService : IRequestLoggerService
    {
        private readonly ILogger<RequestLoggerService> logger;
        private readonly IUserContextService userContextService;

        public RequestLoggerService(
            ILogger<RequestLoggerService> logger,
            IUserContextService userContextService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public Task LogRequestAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : notnull
        {
            var requestName = typeof(TRequest).Name;
            var userId = this.userContextService.CurrentContext.UserId;

            var sanitizedObject = request is ISanitizableRequest sanitizable
                ? sanitizable.GetSanitized()
                : request;

            var requestSerialized = JsonSerializer.Serialize(sanitizedObject);

            this.logger.LogDebug("Request: {RequestName} User ID: {@UserId} Request Data: {RequestSerialized}", requestName, userId, requestSerialized);

            return Task.CompletedTask;
        }
    }
}