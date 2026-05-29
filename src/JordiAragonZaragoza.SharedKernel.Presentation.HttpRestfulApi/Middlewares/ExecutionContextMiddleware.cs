namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class ExecutionContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExecutionContextMiddleware> logger;

        public ExecutionContextMiddleware(RequestDelegate next, ILogger<ExecutionContextMiddleware> logger)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(
            HttpContext context,
            IExecutionContextService executionContextService,
            IAuthorizationService authorizationService,
            IServiceIdentityProvider serviceIdentityProvider)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(executionContextService);
            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(serviceIdentityProvider);

            if (AnonymousRequestHelper.IsInfrastructureEndpoint(context))
            {
                await this.next(context);
                return;
            }

            var cancellationToken = context.RequestAborted;
            var correlationId = ResolveCorrelationId(context);
            var causationId = ResolveCausationId(context);
            string executor = serviceIdentityProvider.GetName();

            var (actorId, actorType) = ResolveActor(context);

            // For external actors (registration, webhooks) the tenant can come
            // in the body/route or in the header. If it doesn't come, we use the system tenant as a fallback
            //  — each endpoint decides if it requires it.
            var tenantId = ResolveTenantId(context);

            if (tenantId == Guid.Empty && actorType != ActorType.External)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    new { error = "invalid_tenant", message = "x-tenant-id header is required." },
                    cancellationToken);
                return;
            }

            var effectiveTenantId = tenantId == Guid.Empty
                ? SystemConstants.SystemTenantId
                : tenantId;

            var partitionId = ResolveOptionalGuid(context, "x-partition-id");
            var domainId = ResolveOptionalGuid(context, "x-domain-id");

            using (this.logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["ActorId"] = actorId,
                ["ActorType"] = actorType.Name,
                ["Executor"] = executor,
                ["ExecutorType"] = ExecutorType.Service.Name,
                ["TenantId"] = effectiveTenantId,
                ["PartitionId"] = partitionId,
                ["DomainId"] = domainId,
            }))
            {
                var scopeContext = new ScopeContext(effectiveTenantId, partitionId, domainId);
                var executionContext = new ExecutionContext(
                    actorId,
                    actorType,
                    executor,
                    ExecutorType.Service,
                    correlationId,
                    causationId,
                    scopeContext);

                // La validación de scope solo aplica a actores User conocidos.
                // External no tiene userId en nuestro sistema todavía.
                if (actorType == ActorType.User)
                {
                    var accessResult = await authorizationService.ValidateScopeAsync(
                        userId: executionContext.GetUserActorId(),
                        scope: scopeContext,
                        cancellationToken);

                    if (!accessResult.IsSuccess)
                    {
                        this.logger.LogWarning(
                            "Authorization failed for Actor {ActorId} on Tenant {TenantId}",
                            actorId,
                            effectiveTenantId);

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(
                            new
                            {
                                error = "forbidden",
                                message = accessResult.Errors.FirstOrDefault()?.ToString() ?? "Access denied.",
                            },
                            cancellationToken);
                        return;
                    }
                }

                executionContextService.SetExecutionContext(executionContext);

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
        }

        /// <summary>
        /// Returns the actor identity. For authenticated users, extracts the 'oid' claim.
        /// For unauthenticated requests (register, webhooks), returns an External actor
        /// derived from the client IP — providing traceability without a JWT.
        /// </summary>
        private static (string ActorId, ActorType ActorType) ResolveActor(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var oid = context.User.FindFirst("oid")?.Value;
                if (Guid.TryParse(oid, out var parsed))
                {
                    return (ExecutionContext.CreateUserActorId(parsed), ActorType.User);
                }
            }

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return (ExecutionContext.CreateExternalActorId(clientIp), ActorType.External);
        }

        private static Guid ResolveTenantId(HttpContext context)
        {
            var header = context.Request.Headers["x-tenant-id"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty ? parsed : Guid.Empty;
        }

        private static Guid? ResolveOptionalGuid(HttpContext context, string headerName)
        {
            var header = context.Request.Headers[headerName].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty ? parsed : null;
        }

        private static Guid ResolveCorrelationId(HttpContext context)
        {
            var header = context.Request.Headers["x-correlation-id"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty ? parsed : Guid.NewGuid();
        }

        private static Guid? ResolveCausationId(HttpContext context)
        {
            var header = context.Request.Headers["x-causation-id"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty ? parsed : null;
        }
    }
}