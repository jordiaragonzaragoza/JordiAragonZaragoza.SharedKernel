namespace JordiAragonZaragoza.SharedKernel.Application.Validators
{
    using FluentValidation;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;

    public abstract class BasePaginatedQueryValidator<TQuery> : BaseValidator<TQuery>
        where TQuery : IPaginatedQuery
    {
        protected BasePaginatedQueryValidator()
        {
            _ = this.RuleFor(static x => x.PageNumber)
                .Must(static pageNumber => pageNumber >= 0)
                .WithMessage("PageNumber must be greater than or equal to 0.");

            _ = this.RuleFor(static x => x.PageSize)
                .Must(static pageSize => pageSize >= 0)
                .WithMessage("PageSize must be greater than or equal to 0.");
        }
    }
}