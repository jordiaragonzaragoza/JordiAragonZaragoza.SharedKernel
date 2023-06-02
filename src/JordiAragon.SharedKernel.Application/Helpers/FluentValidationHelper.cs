﻿namespace JordiAragon.SharedKernel.Application.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.Result;
    using Ardalis.Result.FluentValidation;
    using FluentValidation;
    using FluentValidation.Results;

    public static class FluentValidationHelper
    {
        // TODO: Temporal. Remove when all validations where will be moved to Application Layer.
        public static IDictionary<string, string[]> FormatResponse(this ValidationResult result)
        {
            return result.Errors.GroupBy(x => x.PropertyName, x => x.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray());
        }

        public static List<ValidationError> AsErrors(this List<ValidationFailure> valResult)
        {
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