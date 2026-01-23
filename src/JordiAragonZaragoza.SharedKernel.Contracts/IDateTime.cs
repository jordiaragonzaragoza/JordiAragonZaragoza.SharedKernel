namespace JordiAragonZaragoza.SharedKernel.Contracts
{
    using System;

    public interface IDateTime
    {
        public DateTimeOffset UtcNow { get; }
    }
}