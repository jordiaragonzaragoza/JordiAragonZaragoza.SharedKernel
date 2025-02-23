namespace JordiAragonZaragoza.SharedKernel.Infrastructure.IdGenerator.VoloAbpGuids
{
    using System;
    using Ardalis.GuardClauses;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using Volo.Abp.Guids;

    public class VoloAbpGuidsIdGenerator : IIdGenerator, ITransientDependency
    {
        private readonly IGuidGenerator guidGenerator;

        public VoloAbpGuidsIdGenerator(IGuidGenerator guidGenerator)
        {
            this.guidGenerator = Guard.Against.Null(guidGenerator, nameof(guidGenerator));
        }

        public Guid Create()
        {
            return this.guidGenerator.Create();
        }
    }
}