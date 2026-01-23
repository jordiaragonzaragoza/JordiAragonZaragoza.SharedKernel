namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RabbitMqOptions
    {
        [Required]
        public Uri Url { get; init; } = default!;
    }
}