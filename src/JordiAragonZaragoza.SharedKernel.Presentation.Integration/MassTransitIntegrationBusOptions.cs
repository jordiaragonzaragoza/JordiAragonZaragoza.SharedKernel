namespace JordiAragonZaragoza.SharedKernel.Presentation.Integration
{
    using System.ComponentModel.DataAnnotations;

    public class MassTransitIntegrationBusOptions
    {
        public const string Section = "MassTransit:IntegrationBus";

        [Required]
        public RabbitMqOptions RabbitMq { get; init; } = default!;

        public uint? MaximumConcurrencyLevel { get; init; }

        public uint NumberOfRetries { get; init; } = 2;
    }
}