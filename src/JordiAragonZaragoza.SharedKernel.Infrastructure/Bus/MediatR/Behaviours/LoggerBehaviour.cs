namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using global::MediatR.Pipeline;

    /// <summary>
    /// Defined a request pre-processor for a handler.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    public class LoggerBehaviour<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        private readonly IRequestLoggerService requestLoggerService;

        public LoggerBehaviour(
            IRequestLoggerService requestLoggerService)
        {
            this.requestLoggerService = requestLoggerService
                ?? throw new ArgumentNullException(nameof(requestLoggerService));
        }

        /// <summary>
        /// Process method executes before calling the Handle method on your handler.
        /// </summary>
        /// <param name="request">Incoming request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        public Task Process(TRequest request, CancellationToken cancellationToken)
            => this.requestLoggerService.LogRequestAsync(request, cancellationToken);
    }
}