﻿namespace JordiAragon.SharedKernel.Infrastructure.EntityFramework.Idempotency
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using JordiAragon.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Contracts.Repositories;

    public class IdempotencyService : IIdempotencyService
    {
        private readonly ISpecificationReadRepository<IdempotentConsumer, Guid> specificationRepository;
        private readonly IRepository<IdempotentConsumer, Guid> repository;

        public IdempotencyService(
            ISpecificationReadRepository<IdempotentConsumer, Guid> specificationRepository,
            IRepository<IdempotentConsumer, Guid> repository)
        {
            this.specificationRepository = Guard.Against.Null(specificationRepository, nameof(specificationRepository));
            this.repository = Guard.Against.Null(repository, nameof(repository));
        }

        public async Task<bool> IsProcessedAsync(Guid messageId, string consumerFullName, CancellationToken cancellationToken)
        {
            var existingConsumer = await this.specificationRepository.FirstOrDefaultAsync(new IdempotentConsumerByMessageIdConsumerFullNameSpec(messageId, consumerFullName), cancellationToken);
            if (existingConsumer is null)
            {
                return false;
            }

            return true;
        }

        public async Task MarkAsProcessedAsync(Guid messageId, string consumerFullName, CancellationToken cancellationToken)
        {
            await this.repository.AddAsync(new IdempotentConsumer(Guid.NewGuid(), messageId, consumerFullName), cancellationToken);
        }
    }
}