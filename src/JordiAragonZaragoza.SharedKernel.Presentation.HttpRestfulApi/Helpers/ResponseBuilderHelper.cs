namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public static class ResponseBuilderHelper
    {
        private const string TraceId = "traceId";

        public static object BuildResponse(IReadOnlyCollection<ValidationFailure> failures, HttpContext context, int statusCode)
        {
            ArgumentNullException.ThrowIfNull(failures, nameof(failures));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            switch (statusCode)
            {
                case (int)HttpStatusCode.UnprocessableEntity:
                    return UnprocessableEntity(failures, context, statusCode);

                case (int)HttpStatusCode.NotFound:
                    return NotFound(failures, context, statusCode);

                case (int)HttpStatusCode.Conflict:
                    return Conflict(failures, context, statusCode);

                default:
                    return Invalid(failures, context, statusCode);
            }
        }

        private static ValidationProblemDetails Invalid(IReadOnlyCollection<ValidationFailure> failures, HttpContext context, int statusCode)
        {
            return new ValidationProblemDetails(
                failures.GroupBy(static f => f.PropertyName)
                    .ToDictionary(
                        keySelector: static e => e.Key,
                        elementSelector: static e => e.Select(static m => m.ErrorMessage).ToArray()))
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = statusCode,
                Instance = context.Request.Path,
                Extensions = { { TraceId, context.TraceIdentifier } },
            };
        }

        private static ProblemDetails Conflict(IReadOnlyCollection<ValidationFailure> failures, HttpContext context, int statusCode)
        {
            return new ProblemDetails
            {
                Type = "https://www.rfc-editor.org/rfc/rfc9110#name-409-conflict",
                Title = "There was a conflict.",
                Status = statusCode,
                Instance = context.Request.Path,
                Extensions = { { TraceId, context.TraceIdentifier } },
                Detail = failures.Count > 0 ? PrepareDetails(failures) : null,
            };
        }

        private static ProblemDetails UnprocessableEntity(IReadOnlyCollection<ValidationFailure> failures, HttpContext context, int statusCode)
        {
            return new ProblemDetails
            {
                Type = "https://www.rfc-editor.org/rfc/rfc9110#name-422-unprocessable-content",
                Title = "Something went wrong.",
                Status = statusCode,
                Instance = context.Request.Path,
                Extensions = { { TraceId, context.TraceIdentifier } },
                Detail = failures.Count > 0 ? PrepareDetails(failures) : null,
            };
        }

        private static ProblemDetails NotFound(IReadOnlyCollection<ValidationFailure> failures, HttpContext context, int statusCode)
        {
            return new ProblemDetails()
            {
                Title = "Resource not found.",
                Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
                Status = statusCode,
                Instance = context.Request.Path,
                Extensions = { { TraceId, context.TraceIdentifier } },
                Detail = failures.Count > 0 ? PrepareDetails(failures) : null,
            };
        }

        private static string PrepareDetails(IReadOnlyCollection<ValidationFailure> failures)
        {
            var details = new StringBuilder("Next error(s) occured:");

            foreach (var error in failures)
            {
                _ = details.Append("* ").Append(error).AppendLine();
            }

            return details.ToString();
        }
    }
}