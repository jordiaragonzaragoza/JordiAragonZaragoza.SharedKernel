namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Outbox
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Contracts.Repositories;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public abstract class ProcessOutboxMessagesJob : IJob
    {
        private readonly IDateTime dateTime;
        private readonly IEventBus eventBus;
        private readonly ILogger<ProcessOutboxMessagesJob> logger;
        private readonly ICachedSpecificationRepository<OutboxMessage, Guid> repositoryOutboxMessages;

        protected ProcessOutboxMessagesJob(
            IDateTime dateTime,
            IEventBus eventBus,
            ILogger<ProcessOutboxMessagesJob> logger,
            ICachedSpecificationRepository<OutboxMessage, Guid> repositoryOutboxMessages)
        {
            this.dateTime = Guard.Against.Null(dateTime, nameof(dateTime));
            this.eventBus = Guard.Against.Null(eventBus, nameof(eventBus));
            this.logger = Guard.Against.Null(logger, nameof(logger));
            this.repositoryOutboxMessages = Guard.Against.Null(repositoryOutboxMessages, nameof(repositoryOutboxMessages));
        }

        protected abstract IEnumerable<Assembly> CurrentAssemblies { get; }

        [SuppressMessage("Design", "CA1031: Do not catch general exception types", Justification = "Ok for this method.")]
        public async Task Execute(IJobExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var outboxMessages = await this.repositoryOutboxMessages.ListAsync(new OutboxMessagesUnProcessedWithoutErrorSpec(), context.CancellationToken);
            if (outboxMessages.Count == 0)
            {
                return;
            }

            foreach (var outboxMessage in outboxMessages)
            {
                var type = this.CurrentAssemblies
                                .Select(assembly => assembly.GetType(outboxMessage.Type))
                                .FirstOrDefault(type => type != null);

                if (type is null)
                {
                    this.logger.LogError(
                        "Error finding type for Outbox Message. Id: {@Id}  Type: {Type} Content:{Content}",
                        outboxMessage.Id,
                        outboxMessage.Type,
                        outboxMessage.Content);

                    continue;
                }

                object? messageDeserialized;
                try
                {
                    messageDeserialized = JsonSerializer.Deserialize(outboxMessage.Content, type);
                }
                catch (Exception exception)
                {
                    this.logger.LogError(
                       exception,
                       "Error Deserializing Outbox Message. Id: {@Id}  Type: {Type} Content: {Content}.",
                       outboxMessage.Id,
                       outboxMessage.Type,
                       outboxMessage.Content);

                    continue;
                }

                if (messageDeserialized is null)
                {
                    this.logger.LogError(
                       "Error Deserializing Outbox Message. Id: {@Id}  Type: {Type} Content: {Content}.",
                       outboxMessage.Id,
                       outboxMessage.Type,
                       outboxMessage.Content);

                    continue;
                }

                if (messageDeserialized is IEventNotification eventNotification)
                {
                    await this.PublishAsync(outboxMessage, eventNotification, context.CancellationToken);
                }
            }

            await this.repositoryOutboxMessages.UpdateRangeAsync(outboxMessages, context.CancellationToken);
        }

        [SuppressMessage("Design", "CA1031: Do not catch general exception types", Justification = "Ok handling any exception from event handlers")]
        private async Task PublishAsync(OutboxMessage outboxMessage, IEventNotification eventNotification, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.eventBus.PublishAsync(eventNotification, cancellationToken);

                outboxMessage.DateProcessedOnUtc = this.dateTime.UtcNow;
            }
            catch (Exception exception)
            {
                outboxMessage.Error = $"Message: {exception.Message}\nStackTrace: {exception.StackTrace}";
            }
        }
    }
}