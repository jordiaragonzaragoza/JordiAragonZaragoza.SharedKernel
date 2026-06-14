namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRequestQueryActivityService<TRequest>
        where TRequest : notnull
    {
        Task<TResponse> ExecuteWithActivityAsync<TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken = default);
    }
}