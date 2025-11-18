namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using Ardalis.Result;
    using global::MassTransit;
    using Microsoft.Extensions.Logging;

    public sealed class UnitOfWorkConsumerFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IDateTime dateTime;
        private readonly ILogger<UnitOfWorkConsumerFilter<T>> logger;

        public UnitOfWorkConsumerFilter(
            IUnitOfWork userContextService,
            IDateTime dateTime,
            ILogger<UnitOfWorkConsumerFilter<T>> logger)
        {
            this.unitOfWork = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            // This transaction is required to commit changes due to use transactional outbox and repositories.
            await this.unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    try
                    {
                        await next.Send(context);

                        this.logger.LogDebug("Consumed message: {Message} at {DateTime}", context.Message.GetType().Name, this.dateTime.UtcNow);
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogError(
                            exception,
                            "Error consuming message: {@Name} {Content} at {DateTime}",
                            context.Message.GetType().Name,
                            context.Message,
                            this.dateTime.UtcNow);

                        throw;
                    }

                    return Result.Success();
                },
                CancellationToken.None);
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("UnitOfWorkConsumerFilter");
    }
}