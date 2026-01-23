namespace JordiAragonZaragoza.SharedKernel.Infrastructure.IdGenerator
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public sealed class IdGeneratorService : IIdGenerator
    {
        public Guid Create()
        {
            return Guid.CreateVersion7();
        }
    }
}