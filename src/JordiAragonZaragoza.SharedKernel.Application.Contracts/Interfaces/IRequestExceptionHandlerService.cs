namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRequestExceptionHandlerService
    {
        Task<TResponse> ExecuteWithExceptionHandlingAsync<TRequest, TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
            where TRequest : notnull;
    }
}