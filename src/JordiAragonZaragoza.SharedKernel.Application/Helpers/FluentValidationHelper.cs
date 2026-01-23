namespace JordiAragonZaragoza.SharedKernel.Application.Helpers
{
    using System;
    using System.Collections.Generic;
    using Ardalis.Result;
    using Ardalis.Result.FluentValidation;
    using FluentValidation;
    using FluentValidation.Results;

    public static class FluentValidationHelper
    {
        public static IReadOnlyCollection<ValidationError> AsErrors(this List<ValidationFailure> valResult)
        {
            ArgumentNullException.ThrowIfNull(valResult);

            var resultErrors = new List<ValidationError>();

            foreach (var valFailure in valResult)
            {
                resultErrors.Add(new ValidationError()
                {
                    Severity = FluentValidationResultExtensions.FromSeverity(valFailure.Severity),
                    ErrorMessage = valFailure.ErrorMessage,
                    ErrorCode = valFailure.ErrorCode,
                    Identifier = valFailure.PropertyName,
                });
            }

            return resultErrors;
        }

        public static IRuleBuilderOptions<T, TProperty> PropertyName<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName), "A property name must be specified when calling UsePropertyName.");
            }

            DefaultValidatorOptions.Configurable(rule).PropertyName = propertyName.ToLowerFirstCharacter();
            return rule;
        }
    }
}