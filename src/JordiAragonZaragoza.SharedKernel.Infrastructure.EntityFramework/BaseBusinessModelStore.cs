namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Contracts.Events;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public abstract class BaseBusinessModelStore : IAggregatesStore, IUnitOfWork, IDisposable, IScopedDependency
    {
        private readonly BaseBusinessModelContext writeContext;
        private IDbContextTransaction transaction = default!;
        private bool disposed;

        protected BaseBusinessModelStore(BaseBusinessModelContext writeContext)
        {
            this.writeContext = Guard.Against.Null(writeContext, nameof(writeContext));
        }

        public IEnumerable<IEventsContainer<IEvent>> EventableEntities
            => this.writeContext.ChangeTracker.Entries<IEventsContainer<IDomainEvent>>()
                            .Select(static e => e.Entity)
                            .Where(static entity => entity.Events.Any());

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> operation)
            where TResponse : IResult
        {
            var strategy = this.writeContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Start transaction inside strategy
                this.transaction ??= await this.writeContext.Database.BeginTransactionAsync();

                try
                {
                    ArgumentNullException.ThrowIfNull(operation, nameof(operation));

                    // Execute operation
                    var response = await operation();

                    // Get Ardalis.Result.IsSuccess or Ardalis.Result<T>.IsSuccess
                    var isSuccessResponse = typeof(TResponse).GetProperty("IsSuccess")?.GetValue(response, null) ?? false;
                    if ((bool)isSuccessResponse)
                    {
                        await this.transaction.CommitAsync();
                    }
                    else
                    {
                        await this.transaction.RollbackAsync();
                    }

                    return response;
                }
                catch
                {
                    await this.transaction.RollbackAsync();
                    throw;
                }
                finally
                {
                    this.transaction.Dispose();
                    this.transaction = null!;
                }
            });
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