namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration.Producers.MassTransit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using Ardalis.Result;
    using global::MassTransit;
    using global::MassTransit.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MassTransitIntegrationQueryBus : IIntegrationQueryBus
    {
        private readonly Bind<IIntegrationBus, IClientFactory> clientFactory;
        private readonly ILogger<MassTransitIntegrationQueryBus> logger;
        private readonly IDateTime dateTime;

        public MassTransitIntegrationQueryBus(
            Bind<IIntegrationBus, IClientFactory> clientFactory,
            ILogger<MassTransitIntegrationQueryBus> logger,
            IDateTime dateTime)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<Result<TResponse>> SendAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : class, IIntegrationQuery
            where TResponse : notnull
        {
            ArgumentNullException.ThrowIfNull(query);

            var client = this.clientFactory.Value.CreateRequestClient<TQuery>();
            query.DateDispatchedOnUtc = this.dateTime.UtcNow;

            try
            {
                this.logger.LogDebug("Sent Integration query: {Query} {Id}", query.GetType().Name, query.Id);

                var response = await client.GetResponse<Result<TResponse>>(query, cancellationToken);

                return response.Message;
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                   exception,
                   "Error sending integration query: {@Name} {Id} {Content} at {DateTime}",
                   query.GetType().Name,
                   query.Id,
                   query,
                   this.dateTime.UtcNow);

                throw;
            }
        }
    }
}