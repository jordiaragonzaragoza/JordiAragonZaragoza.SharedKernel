namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb
{
    using FluentValidation;

    public class EventStoreDbOptionsValidator : AbstractValidator<EventStoreDbOptions>
    {
        public EventStoreDbOptionsValidator()
        {
            _ = this.RuleFor(static x => x.ConnectionString)
                .NotEmpty();
        }
    }
}