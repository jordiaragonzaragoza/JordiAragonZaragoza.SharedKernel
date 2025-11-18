namespace JordiAragonZaragoza.SharedKernel.Application.Handlers
{
    using System;
    using System.Collections.Generic;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public static class AsyncCommandHandlerRegistry
    {
        private static readonly Dictionary<Type, Type> AsyncCommandHandlerTypes = [];

        public static void Register<TAsyncCommand, TAsyncCommandHandler>()
            where TAsyncCommand : IAsyncCommand
            where TAsyncCommandHandler : IAsyncCommandHandler
        {
            AsyncCommandHandlerTypes[typeof(TAsyncCommand)] = typeof(TAsyncCommandHandler);
        }

        public static Type GetAsyncCommandHandlerType(IAsyncCommand asyncCommand)
        {
            ArgumentNullException.ThrowIfNull(asyncCommand);

            var asyncCommandType = asyncCommand.GetType();
            if (AsyncCommandHandlerTypes.TryGetValue(asyncCommandType, out var asyncCommandHandlerType))
            {
                return asyncCommandHandlerType;
            }

            throw new InvalidOperationException($"async command handler registered for {asyncCommand.GetType()}.");
        }
    }
}