namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Outbox
{
    using System.Linq;
    using Ardalis.Specification;

    public class OutboxMessagesUnProcessedWithoutErrorSpec : Specification<OutboxMessage>
    {
        public OutboxMessagesUnProcessedWithoutErrorSpec()
        {
            _ = this.Query.Where(static outboxMessage => outboxMessage.DateProcessedOnUtc == null && outboxMessage.Error == string.Empty);
        }
    }
}