namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Outbox
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Contracts.Outbox;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class OutboxService : IOutboxService
    {
        private readonly IIdGenerator guidGenerator;
        private readonly ICachedSpecificationRepository<OutboxMessage, Guid> repositoryOutboxMessage;
        private readonly ILogger<OutboxService> logger;

        public OutboxService(
            IIdGenerator guidGenerator,
            ICachedSpecificationRepository<OutboxMessage, Guid> repositoryOutboxMessage,
            ILogger<OutboxService> logger)
        {
            this.guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
            this.repositoryOutboxMessage = repositoryOutboxMessage ?? throw new ArgumentNullException(nameof(repositoryOutboxMessage));
            this.logger = logger;
        }

        public async Task AddMessageAsync(IEventNotification<IEvent> eventNotification, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventNotification);

            var type = eventNotification.GetType().FullName
                ?? throw new InvalidOperationException("The full name of the eventNotification is null.");

            var data = JsonConvert.SerializeObject(eventNotification); // TODO: Change to System.Text.Json when nested serialization is supported.

            var outboxMessage = new OutboxMessage(
                this.guidGenerator.Create(),
                eventNotification.Event.DateOccurredOnUtc,
                type,
                data);

            _ = await this.repositoryOutboxMessage.AddAsync(outboxMessage, cancellationToken);

            this.logger.LogInformation("Stored Notification Event: {Event}", eventNotification.GetType().Name);
        }
    }
}