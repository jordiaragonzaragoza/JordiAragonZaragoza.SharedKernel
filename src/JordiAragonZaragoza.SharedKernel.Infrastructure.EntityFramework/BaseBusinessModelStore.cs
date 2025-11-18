namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public abstract class BaseBusinessModelStore : IAggregateStore, IUnitOfWork, IDisposable
    {
        private readonly BaseBusinessModelContext writeContext;
        private IDbContextTransaction transaction = default!;
        private bool disposed;

        protected BaseBusinessModelStore(BaseBusinessModelContext writeContext)
        {
            this.writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
        }

        public IEnumerable<IEventsContainer<IEvent>> EventableEntities
            => this.writeContext.ChangeTracker.Entries<IEventsContainer<IDomainEvent>>()
                            .Select(static e => e.Entity)
                            .Where(static entity => entity.Events.Any());

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> operation, CancellationToken cancellationToken = default)
            where TResponse : IResult
        {
            var strategy = this.writeContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(
                async ct =>
                {
                    // Start transaction inside strategy
                    this.transaction ??= await this.writeContext.Database.BeginTransactionAsync(ct);

                    try
                    {
                        ArgumentNullException.ThrowIfNull(operation, nameof(operation));

                        // Execute operation
                        var response = await operation();

                        // Get Ardalis.Result.IsSuccess or Ardalis.Result<T>.IsSuccess
                        var isSuccessResponse = typeof(TResponse).GetProperty("IsSuccess")?.GetValue(response, null) ?? false;
                        if ((bool)isSuccessResponse)
                        {
                            await this.transaction.CommitAsync(ct);
                        }
                        else
                        {
                            await this.transaction.RollbackAsync(ct);
                        }

                        return response;
                    }
                    catch
                    {
                        await this.transaction.RollbackAsync(ct);
                        throw;
                    }
                    finally
                    {
                        this.transaction.Dispose();
                        this.transaction = null!;
                    }
                },
                cancellationToken);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.transaction?.Dispose();
                this.writeContext?.Dispose();

                this.transaction = null!;
            }

            this.disposed = true;
        }
    }
}