namespace JordiAragonZaragoza.SharedKernel.Application.ReadModels
{
    using System;

    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public abstract class BaseReadModel : IReadModel
    {
        public Guid Id { get; protected set; }

        public ScopeInfo Scope { get; set; } = default!;
    }
}