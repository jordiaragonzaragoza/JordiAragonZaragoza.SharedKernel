namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit
{
    using System.ComponentModel.DataAnnotations;

    public class MassTransitInternalBusOptions
    {
        public const string Section = "MassTransit:Bus";

        [Required]
        public RabbitMqOptions RabbitMq { get; init; } = default!;

        public uint? MaximumConcurrencyLevel { get; init; }

        public uint NumberOfRetries { get; init; } = 2;
    }
}