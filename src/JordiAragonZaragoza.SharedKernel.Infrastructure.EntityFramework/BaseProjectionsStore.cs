namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public abstract class BaseProjectionsStore : IUnitOfWork, IDisposable
    {
        private readonly BaseReadModelContext readContext;
        private IDbContextTransaction transaction = default!;
        private bool disposed;

        protected BaseProjectionsStore(BaseReadModelContext readContext)
        {
            this.readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
        }

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> operation, CancellationToken cancellationToken = default)
            where TResponse : IResult
        {
            var strategy = this.readContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(
                async ct =>
                {
                    // Start transaction inside strategy
                    this.transaction ??= await this.readContext.Database.BeginTransactionAsync(ct);

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
                this.readContext?.Dispose();

                this.transaction = null!;
            }

            this.disposed = true;
        }
    }
}