namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Cache
{
    using FluentValidation;

    public class CacheOptionsValidator : AbstractValidator<CacheOptions>
    {
        public CacheOptionsValidator()
        {
            _ = this.RuleFor(static x => x.DefaultName)
                .NotEmpty();

            _ = this.RuleFor(static x => x.DefaultExpirationInSeconds)
                .GreaterThan(0);
        }
    }
}