// TODO: Temporal remove till review options pattern with validations
/*namespace JordiAragonZaragoza.SharedKernel.Presentation.IntegrationConsumers
{
    using FluentValidation;

    public class RabbitMqOptionsValidator : AbstractValidator<RabbitMqOptions>
    {
        public RabbitMqOptionsValidator()
        {
            _ = this.RuleFor(static x => x.Host)
                .NotEmpty();

            _ = this.RuleFor(static x => x.Username)
                .NotEmpty();

            _ = this.RuleFor(static x => x.Password)
                .NotEmpty();
        }
    }
}*/