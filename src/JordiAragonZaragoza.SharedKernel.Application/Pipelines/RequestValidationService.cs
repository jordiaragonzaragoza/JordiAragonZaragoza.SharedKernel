namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Application.Helpers;
    using Ardalis.Result;
    using FluentValidation;
    using Microsoft.Extensions.Logging;

    public class RequestValidationService<TRequest> : IRequestValidationService<TRequest>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly ILogger<RequestValidationService<TRequest>> logger;
        private readonly IExecutionContextService userContextService;

        public RequestValidationService(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<RequestValidationService<TRequest>> logger,
            IExecutionContextService userContextService)
        {
            this.validators = validators ?? throw new ArgumentNullException(nameof(validators));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }

        public async Task<Result> TryValidateAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            if (!this.validators.Any())
            {
                return Result.Success();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                this.validators.Select(validator =>
                    validator.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                    .Where(validationResult => validationResult.Errors.Count > 0)
                    .SelectMany(validationResult => validationResult.Errors)
                    .ToList();

            if (failures.Count > 0)
            {
                var requestName = typeof(TRequest).Name;
                var userId = this.userContextService.CurrentContext?.ActorId;
                var sanitizedObject = request is ISanitizableRequest sanitizable
                    ? sanitizable.GetSanitized()
                    : request;

                var requestSerialized = JsonSerializer.Serialize(sanitizedObject);
                var errors = failures.AsErrors();
                var errorsSerialized = JsonSerializer.Serialize(errors);

                this.logger.LogInformation("Bad Request: {RequestName} User ID: {@UserId} Request Data: {RequestSerialized} Validation Errors: {ErrorsSerialized}", requestName, userId, requestSerialized, errorsSerialized);

                return Result.Invalid(errors);
            }

            return Result.Success();
        }
    }
}