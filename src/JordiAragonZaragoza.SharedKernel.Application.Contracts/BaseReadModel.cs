namespace JordiAragonZaragoza.SharedKernel.Application.Contracts
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public abstract record class BaseReadModel : IReadModel
    {
        protected BaseReadModel(
            Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must not be empty.", nameof(id));
            }

            this.Id = id;
        }

        public Guid Id { get; private set; }

        public uint Version { get; set; }
    }
}