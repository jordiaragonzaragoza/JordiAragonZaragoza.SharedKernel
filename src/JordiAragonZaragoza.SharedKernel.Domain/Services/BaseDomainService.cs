namespace JordiAragonZaragoza.SharedKernel.Domain.Services
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Exceptions;

    public abstract class BaseDomainService : IDomainService
    {
        protected static void CheckRule(IBusinessRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            if (rule.IsBroken())
            {
                throw new BusinessRuleValidationException(rule);
            }
        }
    }
}