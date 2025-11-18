namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts.Model;

    /// <summary>
    /// Generic abstraction for a application read model.
    /// </summary>
    public interface IReadModel : IBaseModel<Guid>
    {
    }
}