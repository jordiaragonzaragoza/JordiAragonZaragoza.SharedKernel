namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using global::MassTransit;

    public sealed class UserContextProducerFilter<T>
        : IFilter<PublishContext<T>>, IFilter<SendContext<T>>
        where T : class
    {
        private readonly IUserContextService userContextService;

        public UserContextProducerFilter(
            IUserContextService userContextService)
        {
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            this.SetHeaders(context);

            return next.Send(context);
        }

        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            this.SetHeaders(context);

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("UserContextProducerFilter");

        private void SetHeaders(SendContext context)
        {
            try
            {
                var userContext = this.userContextService.CurrentContext;

                context.Headers.Set(UserConstants.UserId, userContext.UserId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("User context must be set before publishing or sending a message.", ex);
            }
        }
    }
}