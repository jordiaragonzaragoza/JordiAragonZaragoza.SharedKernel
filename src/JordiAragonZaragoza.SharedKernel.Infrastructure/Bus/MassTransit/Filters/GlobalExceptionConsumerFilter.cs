namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading.Tasks;
    using global::MassTransit;
    using Microsoft.Extensions.Logging;

    public sealed class GlobalExceptionConsumerFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly ILogger<GlobalExceptionConsumerFilter<T>> logger;

        public GlobalExceptionConsumerFilter(
            ILogger<GlobalExceptionConsumerFilter<T>> logger)
        {
            this.logger = logger;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            try
            {
                await next.Send(context);
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    "Unhandled exception when consuming message of type {MessageType}. Message: {@Message}",
                    typeof(T).Name,
                    context.Message);

                throw;
            }
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("GlobalExceptionConsumeFilter");
    }
}