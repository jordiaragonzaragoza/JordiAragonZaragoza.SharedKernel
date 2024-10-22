﻿namespace JordiAragon.SharedKernel.Application.Events.Decorators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using JordiAragon.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Contracts.Events;
    using JordiAragon.SharedKernel.Domain.Contracts.Interfaces;
    using MediatR;

    /// <summary>
    /// This class allows to dispatch domain events generated by event notification handlers in an idempotent manner.
    /// It serves as a decorator for notification handlers, ensuring that events are handled consistently and without duplication.
    /// </summary>
    /// <typeparam name="TEventNotification">The type of event notification to be handled.</typeparam>
    public class EventNotificationHandlerDecorator<TEventNotification> : INotificationHandlerDecorator<TEventNotification>
        where TEventNotification : IEventNotification
    {
        private readonly INotificationHandler<TEventNotification> decoratedHandler;
        private readonly IEventsDispatcherService domainEventsDispatcher;
        private readonly IIdempotencyService idempotencyService;

        public EventNotificationHandlerDecorator(
            IIdempotencyService idempotencyService,
            IEventsDispatcherService domainEventsDispatcher,
            INotificationHandler<TEventNotification> decoratedHandler)
        {
            this.domainEventsDispatcher = Guard.Against.Null(domainEventsDispatcher, nameof(domainEventsDispatcher));
            this.decoratedHandler = Guard.Against.Null(decoratedHandler, nameof(decoratedHandler));
            this.idempotencyService = Guard.Against.Null(idempotencyService, nameof(idempotencyService));
        }

        public INotificationHandler<TEventNotification> DecoratedHandler
            => this.decoratedHandler;

        public async Task Handle(TEventNotification notification, CancellationToken cancellationToken)
        {
            var messageId = notification.Id;
            var consumerFullName = this.decoratedHandler.GetType().FullName
                ?? throw new InvalidOperationException("The full name of the consumer handler is null.");

            var processed = await this.idempotencyService.IsProcessedAsync(messageId, consumerFullName, cancellationToken);
            if (processed)
            {
                return;
            }

            await this.decoratedHandler.Handle(notification, cancellationToken).ConfigureAwait(true);

            await this.domainEventsDispatcher.DispatchEventsFromAggregatesStoreAsync(cancellationToken);

            await this.idempotencyService.MarkAsProcessedAsync(messageId, consumerFullName, cancellationToken);
        }
    }
}