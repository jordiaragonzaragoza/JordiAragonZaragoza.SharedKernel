namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RabbitMqOptions
    {
        [Required]
        public Uri Url { get; init; } = default!;
    }
}