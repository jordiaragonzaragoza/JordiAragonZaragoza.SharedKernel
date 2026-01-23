namespace JordiAragonZaragoza.SharedKernel.Domain.Exceptions
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Temporal suppresion")]
    public class BusinessRuleValidationException : Exception
    {
        public BusinessRuleValidationException(IBusinessRule brokenRule)
            : base(brokenRule?.Message)
        {
            this.BrokenRule = brokenRule ?? throw new ArgumentNullException(nameof(brokenRule));
        }

        public IBusinessRule BrokenRule { get; }

        public override string ToString()
        {
            return $"{this.BrokenRule.GetType().FullName}: {this.BrokenRule.Message}";
        }
    }
}