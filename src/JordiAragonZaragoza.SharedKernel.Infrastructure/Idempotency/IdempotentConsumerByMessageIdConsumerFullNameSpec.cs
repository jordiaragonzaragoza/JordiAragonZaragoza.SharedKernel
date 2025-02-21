namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Idempotency
{
    using System;
    using System.Linq;
    using Ardalis.GuardClauses;
    using Ardalis.Specification;

    public class IdempotentConsumerByMessageIdConsumerFullNameSpec : SingleResultSpecification<IdempotentConsumer>
    {
        public IdempotentConsumerByMessageIdConsumerFullNameSpec(Guid messageId, string consumerFullName)
        {
            _ = Guard.Against.Default(messageId);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(consumerFullName);

            _ = this.Query
                .Where(consumer => consumer.MessageId == messageId
                                && consumer.ConsumerFullName == consumerFullName);
        }
    }
}