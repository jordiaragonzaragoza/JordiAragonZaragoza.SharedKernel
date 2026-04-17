namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.AspNetCore.Http;

    public sealed class ExecutionContextMiddleware
    {
        private readonly RequestDelegate next;

        public ExecutionContextMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IExecutionContextService executionContextService, IAuthorizationService authorizationService)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(executionContextService);
            ArgumentNullException.ThrowIfNull(authorizationService);

            if (AnonymousRequestHelper.IsAnonymousAllowed(context))
            {
                await this.next(context);

                return;
            }

            var actorId = ResolveActorId(context);
            if (actorId is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "unauthorized",
                    message = "Authentication is required.",
                });

                return;
            }

            var actorType = ResolveActorType(context);

            string executor = string.Empty; // TODO: Resolve executor from context or configuration?
            var executorType = ExecutorType.Service;

            var correlationId = ResolveCorrelationId(context);

            var tenantIdHeader = context.Request.Headers["x-tenant-id"].FirstOrDefault();
            if (!Guid.TryParse(tenantIdHeader, out var tenantId) || tenantId == Guid.Empty)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_tenant",
                    message = "x-tenant-id header is required and must be a valid Guid.",
                });

                return;
            }

            var partitionHeader = context.Request.Headers["x-partition-id"].FirstOrDefault();
            Guid? partitionId = Guid.TryParse(partitionHeader, out var partition) ? partition : null;

            var domainHeader = context.Request.Headers["x-domain-id"].FirstOrDefault();
            Guid? domainId = Guid.TryParse(domainHeader,  out var domain) ? domain : null;

            var newExecutionContext = new ExecutionContext(
                actorId,
                actorType,
                executor,
                executorType,
                correlationId,
                causationId: null,
                new ScopeContext(tenantId, partitionId, domainId));

            executionContextService.SetExecutionContext(
                            newExecutionContext);

            var accessResult = await authorizationService.ValidateScopeAsync(newExecutionContext);

            if (!accessResult.IsSuccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "forbidden",
                    message = accessResult.Errors.FirstOrDefault()?.ToString() ?? "Access denied.",
                });

                return;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd("x-correlation-id", correlationId.ToString());

                return Task.CompletedTask;
            });

            try
            {
                await this.next(context);
            }
            finally
            {
                executionContextService.ClearExecutionContext();
            }
        }

        private static string? ResolveActorId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var oid = context.User.FindFirst("oid")?.Value;
                return Guid.TryParse(oid, out _) ? oid : null;
            }

            return null;
        }

        private static ActorType ResolveActorType(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                return ActorType.User;
            }

            throw new UnauthorizedAccessException("Actor type cannot be resolved.");
        }

        private static Guid ResolveCorrelationId(HttpContext context)
        {
            var correlationIdHeader = context.Request.Headers["x-correlation-id"].FirstOrDefault();

            return Guid.TryParse(correlationIdHeader, out var parsed)
                ? parsed
                : Guid.NewGuid();
        }
    }
}