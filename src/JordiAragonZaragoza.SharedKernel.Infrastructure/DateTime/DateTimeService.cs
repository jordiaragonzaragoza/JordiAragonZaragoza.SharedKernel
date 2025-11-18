namespace JordiAragonZaragoza.SharedKernel.Infrastructure.DateTime
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Contracts;

    public class DateTimeService : IDateTime
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}