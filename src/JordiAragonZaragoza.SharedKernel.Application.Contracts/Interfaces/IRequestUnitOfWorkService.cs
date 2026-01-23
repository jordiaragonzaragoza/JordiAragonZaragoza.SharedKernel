namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;

    public interface IRequestUnitOfWorkService
    {
        Task<TResponse> HandleWithTransactionAsync<TResponse>(
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken = default)
            where TResponse : IResult;
    }
}