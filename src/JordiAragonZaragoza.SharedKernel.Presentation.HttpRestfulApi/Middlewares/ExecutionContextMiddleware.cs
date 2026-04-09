namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
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

            var actorId = ResolveActorId(context);
            if (actorId is null)
            {
                if (AnonymousRequestHelper.IsAnonymousAllowed(context))
                {
                    actorId = ActorConstants.Anonymous;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("x-actor-id header is required for this request.");

                    return;
                }
            }

            var actorType = ResolveActorType(context, actorId);

            var correlationId = ResolveCorrelationId(context);

            var tenantIdHeader = context.Request.Headers["x-tenant-id"].FirstOrDefault();
            if (!Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                if (AnonymousRequestHelper.IsAnonymousAllowed(context))
                {
                    tenantId = Guid.Empty;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("x-tenant-id header is required and must be a valid Guid.");

                    return;
                }
            }

            var partitionHeader = context.Request.Headers["x-partition-id"].FirstOrDefault();
            Guid? partitionId = Guid.TryParse(partitionHeader, out var partition) ? partition : null;

            var domainHeader = context.Request.Headers["x-domain-id"].FirstOrDefault();
            Guid? domainId = Guid.TryParse(domainHeader,  out var domain) ? domain : null;

            var accessResult = await authorizationService.ValidateScopeAsync(
                actorId, tenantId, partitionId, domainId);

            if (!accessResult.IsSuccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(accessResult.Errors.FirstOrDefault()?.ToString() ?? "Access denied.");

                return;
            }

            executionContextService.SetExecutionContext(
                actorId,
                actorType,
                correlationId,
                tenantId,
                partitionId,
                domainId,
                causationId: null);

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["x-correlation-id"] = correlationId.ToString();
                return Task.CompletedTask;
            });

            await this.next(context);
        }

        private static string? ResolveActorId(HttpContext context)
        {
            var jwtSub = context.User?.FindFirst("sub")?.Value;
            if (jwtSub is not null)
            {
                return jwtSub;
            }

            return context.Request.Headers["x-actor-id"].FirstOrDefault();
        }

        private static string ResolveActorType(HttpContext context, string actorId)
        {
            var actorTypeHeader = context.Request.Headers["x-actor-type"].FirstOrDefault();
            if (actorTypeHeader is not null)
            {
                return actorTypeHeader;
            }

            if (actorId == ActorConstants.Anonymous)
            {
                return ActorConstants.Anonymous;
            }

            return context.User?.Identity?.IsAuthenticated == true
                ? ActorConstants.User
                : ActorConstants.Anonymous;
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