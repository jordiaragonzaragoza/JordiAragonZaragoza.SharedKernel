namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Ardalis.Result;
    using global::MediatR;

    public class UnitOfWorkBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ITransactionalCommand
        where TResponse : IResult
    {
        private readonly IRequestUnitOfWorkService requestUnitOfWorkService;

        public UnitOfWorkBehaviour(IRequestUnitOfWorkService requestUnitOfWorkService)
        {
            this.requestUnitOfWorkService = requestUnitOfWorkService ?? throw new ArgumentNullException(nameof(requestUnitOfWorkService));
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
            => this.requestUnitOfWorkService.HandleWithTransactionAsync(_ => next(cancellationToken), cancellationToken);
    }
}