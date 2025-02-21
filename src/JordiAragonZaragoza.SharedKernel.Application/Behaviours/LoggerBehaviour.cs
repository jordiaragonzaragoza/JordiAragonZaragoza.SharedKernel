namespace JordiAragonZaragoza.SharedKernel.Application.Behaviours
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using MediatR.Pipeline;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Defined a request pre-processor for a handler.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    public class LoggerBehaviour<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        private readonly ILogger<LoggerBehaviour<TRequest>> logger;
        private readonly ICurrentUserService currentUserService;

        public LoggerBehaviour(
            ILogger<LoggerBehaviour<TRequest>> logger,
            ICurrentUserService currentUserService)
        {
            this.logger = Guard.Against.Null(logger, nameof(logger));
            this.currentUserService = Guard.Against.Null(currentUserService, nameof(currentUserService));
        }

        /// <summary>
        /// Process method executes before calling the Handle method on your handler.
        /// </summary>
        /// <param name="request">Incoming request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = this.currentUserService.UserId ?? string.Empty;
            var requestSerialized = JsonSerializer.Serialize(request);

            this.logger.LogInformation("Request: {RequestName} User ID: {@UserId} Request Data: {RequestSerialized}", requestName, userId, requestSerialized);

            return Task.CompletedTask;
        }
    }
}