namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;

    public class RequestExceptionHandlerService : IRequestExceptionHandlerService
    {
        private readonly ILogger<RequestExceptionHandlerService> logger;
        private readonly IExecutionContextService userContextService;

        public RequestExceptionHandlerService(
            ILogger<RequestExceptionHandlerService> logger,
            IExecutionContextService userContextService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public async Task<TResponse> ExecuteWithExceptionHandlingAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
            where TRequest : notnull
        {
            ArgumentNullException.ThrowIfNull(next);

            try
            {
                return await next(cancellationToken);
            }
            catch (Exception exception)
            {
                var requestName = typeof(TRequest).Name;
                var userId = this.userContextService.CurrentContext.ActorId;

                var sanitizedObject = request is ISanitizableRequest sanitizable
                    ? sanitizable.GetSanitized()
                    : request;

                var requestSerialized = JsonSerializer.Serialize(sanitizedObject);

                this.logger.LogError(
                    exception,
                    "Unhandled Exception. Request: {RequestName} User ID: {@UserId} Request Data: {RequestSerialized}",
                    requestName,
                    userId,
                    requestSerialized);

                throw;
            }
        }
    }
}