namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using global::MassTransit;

    public sealed class UserContextConsumerFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly IUserContextService userContextService;

        public UserContextConsumerFilter(
            IUserContextService userContextService)
        {
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            var userId = context.Headers.Get<string>(UserConstants.UserId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                if (AnonymousMessageHelper.IsAnonymousAllowed(context))
                {
                    userId = UserConstants.AnonymousUser;
                }
                else
                {
                    throw new InvalidOperationException("Missing UserId headers in message.");
                }
            }

            this.userContextService.SetUserContext(userId!);

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
            => context.CreateFilterScope("UserContextConsumerFilter");
    }
}