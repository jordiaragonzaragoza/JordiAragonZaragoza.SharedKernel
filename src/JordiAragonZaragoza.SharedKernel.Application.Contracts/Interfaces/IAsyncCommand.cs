namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    public interface IAsyncCommand
    {
        public Guid Id { get; }
    }
}