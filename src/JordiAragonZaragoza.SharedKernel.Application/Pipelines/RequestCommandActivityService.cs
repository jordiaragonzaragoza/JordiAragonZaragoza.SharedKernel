namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public class RequestCommandActivityService<TRequest> : IRequestCommandActivityService<TRequest>
        where TRequest : notnull
    {
        private static readonly ActivitySource ActivitySource =
            new("JordiAragonZaragoza.SharedKernel.Application.Handlers");

        public async Task<TResponse> ExecuteWithActivityAsync<TResponse>(
            TRequest request,
            Func<CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(next);

            using var activity = ActivitySource.StartActivity(
                $"CommandHandler: {typeof(TRequest).Name}",
                ActivityKind.Internal);

            activity?.SetTag("command.type", typeof(TRequest).FullName);

            return await next(cancellationToken);
        }
    }
}