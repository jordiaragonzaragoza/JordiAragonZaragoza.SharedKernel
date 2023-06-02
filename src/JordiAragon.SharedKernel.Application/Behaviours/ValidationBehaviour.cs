﻿namespace JordiAragon.SharedKernel.Application.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using FluentValidation;
    using JordiAragon.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragon.SharedKernel.Application.Helpers;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
         where TResponse : IResult
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly ILogger<TRequest> logger;
        private readonly ICurrentUserService currentUserService;

        public ValidationBehaviour(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<TRequest> logger,
            ICurrentUserService currentUserService)
        {
            this.validators = validators ?? throw new ArgumentNullException(nameof(validators));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (this.validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    this.validators.Select(validator =>
                        validator.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(validationResult => validationResult.Errors.Any())
                    .SelectMany(validationResult => validationResult.Errors)
                    .ToList();

                if (failures.Any())
                {
                    var requestName = typeof(TRequest).Name;
                    var userId = this.currentUserService.UserId ?? string.Empty;
                    var requestSerialized = JsonSerializer.Serialize(request);
                    var errors = failures.AsErrors();
                    var errorsSerialized = JsonSerializer.Serialize(errors);

                    this.logger.LogInformation("Bad Request: {RequestName} User ID: {@UserId} Request Data: {RequestSerialized} Validation Errors: {errorsSerialized}", requestName, userId, requestSerialized, errorsSerialized);

                    // Get Ardalis.Result.Invalid or Ardalis.Result<T>.Invalid method.
                    var resultInvalidMethod = typeof(TResponse).GetMethod("Invalid", BindingFlags.Static | BindingFlags.Public);

                    return (TResponse)resultInvalidMethod.Invoke(null, new object[] { errors });
                }
            }

            return await next();
        }
    }
}