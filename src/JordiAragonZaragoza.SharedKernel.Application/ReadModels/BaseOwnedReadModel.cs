namespace JordiAragonZaragoza.SharedKernel.Application.ReadModels
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;

    /// <summary>
    /// Base for read model entities that are owned by a root read model.
    /// Owned entities inherit tenant visibility from their parent read model, so
    /// they do not carry their own ScopeInfo.
    /// </summary>
    public abstract class BaseOwnedReadModel : IBaseModel<Guid>
    {
        public Guid Id { get; init; }
    }
}