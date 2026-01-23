namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class PerformancePipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IRequestPerformanceTrackingService performanceTrackingService;

        public PerformancePipelineBehaviour(IRequestPerformanceTrackingService performanceTrackingService)
        {
            this.performanceTrackingService = performanceTrackingService ?? throw new ArgumentNullException(nameof(performanceTrackingService));
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => this.performanceTrackingService.TrackPerformanceAsync(
                request,
                _ => next(cancellationToken),
                cancellationToken);
    }
}