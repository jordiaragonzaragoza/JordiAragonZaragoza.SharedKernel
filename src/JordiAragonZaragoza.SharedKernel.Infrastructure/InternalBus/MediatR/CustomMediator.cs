namespace JordiAragonZaragoza.SharedKernel.Infrastructure.InternalBus.MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using global::MediatR.NotificationPublishers;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    /// <summary>
    /// This class is required due to an issue with Autofac, Decorators and .NET DI.
    /// </summary>
    public class CustomMediator : Mediator
    {
        public CustomMediator(IServiceProvider serviceProvider)
            : base(serviceProvider, new ForeachAwaitPublisher())
        {
        }

        //// Unfortunately Publish() seems to hit the handlers twice!!!
        //// https://github.com/jbogard/MediatR/issues/702
        //// https://github.com/jbogard/MediatR/issues/718
        //// https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/issues/118
        protected override async Task PublishCore(
            IEnumerable<NotificationHandlerExecutor> handlerExecutors,
            INotification notification,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(handlerExecutors, nameof(handlerExecutors));

            var newHandlerExecutors = new List<NotificationHandlerExecutor>();
            foreach (var handler in handlerExecutors)
            {
                var decoratedHander = FindDecoratedHandler(handler.HandlerInstance);
                if (decoratedHander != null)
                {
                    if (!newHandlerExecutors.Exists(n =>
                        n.HandlerInstance.GetType() == handler.HandlerInstance.GetType() &&
                        FindDecoratedHandler(n.HandlerInstance)?.GetType() == decoratedHander.GetType()))
                    {
                        newHandlerExecutors.Add(handler);
                    }

                    continue;
                }

                if (!newHandlerExecutors.Exists(n => n.HandlerInstance.GetType() == handler.HandlerInstance.GetType()))
                {
                    newHandlerExecutors.Add(handler);
                }
            }

            await base.PublishCore(newHandlerExecutors, notification, cancellationToken);
        }

        private static object? FindDecoratedHandler(object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();

            if (Array.Exists(handlerType.GetInterfaces(), static i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(INotificationHandlerDecorator<>)))
            {
                // This handler is a decorator
                var decoratorInterface = Array.Find(handlerType.GetInterfaces(), static i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(INotificationHandlerDecorator<>));

                if (decoratorInterface != null)
                {
                    // Extract the generic argument from the decorator interface
                    var eventType = decoratorInterface.GetGenericArguments()[0];

                    // Construct the generic type of INotificationHandlerDecorator<> with the specific event type
                    var decoratorType = typeof(INotificationHandlerDecorator<>).MakeGenericType(eventType);

                    // Get the Decorated property using reflection
                    return decoratorType.GetProperty("DecoratedHandler")?.GetValue(handlerInstance);
                }
            }

            return null;
        }
    }
}