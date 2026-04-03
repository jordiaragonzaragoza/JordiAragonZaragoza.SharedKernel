namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class RequestPerformanceTrackingService : IRequestPerformanceTrackingService
    {
        private readonly ILogger<RequestPerformanceTrackingService> logger;
        private readonly IExecutionContextService userContextService;

        public RequestPerformanceTrackingService(
            ILogger<RequestPerformanceTrackingService> logger,
            IExecutionContextService userContextService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public async Task<TResponse> TrackPerformanceAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
            where TRequest : notnull
        {
            ArgumentNullException.ThrowIfNull(next);

            var timer = Stopwatch.StartNew();

            var response = await next(cancellationToken);

            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;
            var requestName = typeof(TRequest).Name;
            var userId = this.userContextService.CurrentContext.ActorId;
            var sanitizedObject = request is ISanitizableRequest sanitizable
                ? sanitizable.GetSanitized()
                : request;

            var requestSerialized = JsonSerializer.Serialize(sanitizedObject);

            // Get Ardalis.Result.Value or Ardalis.Result<T>.Value property.
            var responseValue = typeof(TResponse).GetProperty("Value")?.GetValue(response, null);
            var responseSerialized = JsonSerializer.Serialize(responseValue);

            // TODO: Pass this '1500' through IConfiguration
            if (elapsedMilliseconds > 1500)
            {
                this.logger.LogWarning(
                    "Long Running Request: {RequestName} Elapsed Time: {ElapsedMilliseconds}ms User ID: {@UserId} Request: {RequestSerialized}",
                    requestName,
                    elapsedMilliseconds,
                    userId,
                    requestSerialized);
            }

            this.logger.LogDebug(
                "Request Completed: {RequestName} Elapsed Time: {ElapsedMilliseconds}ms User ID: {@UserId} Request: {RequestSerialized} Response: {ResponseSerialized}",
                requestName,
                elapsedMilliseconds,
                userId,
                requestSerialized,
                responseSerialized);

            return response;
        }
    }
}