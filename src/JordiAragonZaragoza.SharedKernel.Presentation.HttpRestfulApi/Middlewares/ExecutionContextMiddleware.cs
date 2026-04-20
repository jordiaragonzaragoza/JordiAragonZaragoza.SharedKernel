namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class ExecutionContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExecutionContextMiddleware> logger;

        public ExecutionContextMiddleware(
            RequestDelegate next,
            ILogger<ExecutionContextMiddleware> logger)
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

            if (AnonymousRequestHelper.IsAnonymousAllowed(context))
            {
                await this.next(context);
                return;
            }

            var actor = ResolveActor(context);
            if (actor is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "unauthorized",
                    message = "Authentication is required.",
                });
                return;
            }

            var (actorId, actorType) = actor.Value;

            var tenantIdHeader = context.Request.Headers["x-tenant-id"].FirstOrDefault();
            if (!Guid.TryParse(tenantIdHeader, out var tenantId) || tenantId == Guid.Empty)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_tenant",
                    message = "x-tenant-id header is required and must be a valid non-empty Guid.",
                });
                return;
            }

            string executor = serviceIdentityProvider.GetName();
            var executorType = ExecutorType.Service;
            var correlationId = ResolveCorrelationId(context);
            var causationId = ResolveCausationId(context);

            var partitionHeader = context.Request.Headers["x-partition-id"].FirstOrDefault();
            Guid? partitionId = Guid.TryParse(partitionHeader, out var partition) && partition != Guid.Empty
                ? partition
                : null;

            var domainHeader = context.Request.Headers["x-domain-id"].FirstOrDefault();
            Guid? domainId = Guid.TryParse(domainHeader, out var domain) && domain != Guid.Empty
                ? domain
                : null;

            using (this.logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["ActorId"] = actorId,
                ["ActorType"] = actorType.Name,
                ["Executor"] = executor,
                ["ExecutorType"] = executorType.Name,
                ["TenantId"] = tenantId,
                ["PartitionId"] = partitionId,
                ["DomainId"] = domainId,
            }))
            {
                var newExecutionContext = new ExecutionContext(
                    actorId,
                    actorType,
                    executor,
                    executorType,
                    correlationId,
                    causationId,
                    new ScopeContext(tenantId, partitionId, domainId));

                var accessResult = await authorizationService.ValidateScopeAsync(newExecutionContext);

                if (!accessResult.IsSuccess)
                {
                    this.logger.LogWarning(
                        "Authorization failed for Actor {ActorId} on Tenant {TenantId}, Partition {PartitionId}, Domain {DomainId}",
                        actorId,
                        tenantId,
                        partitionId,
                        domainId);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "forbidden",
                        message = accessResult.Errors.FirstOrDefault()?.ToString() ?? "Access denied.",
                    });
                    return;
                }

                executionContextService.SetExecutionContext(newExecutionContext);

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

        private static (string ActorId, ActorType ActorType)? ResolveActor(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var oid = context.User.FindFirst("oid")?.Value;
                if (Guid.TryParse(oid, out var parsed))
                {
                    return (ExecutionContext.CreateUserActorId(parsed), ActorType.User);
                }
            }

            return null;
        }

        private static Guid ResolveCorrelationId(HttpContext context)
        {
            var header = context.Request.Headers["x-correlation-id"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty
                ? parsed
                : Guid.NewGuid();
        }

        // Extracts an optional causation id from the incoming request.
        // For plain HTTP requests this will almost always be null; it becomes
        // relevant when an HTTP endpoint is triggered as a reaction to a
        // prior event and the caller forwards the originating event id.
        private static Guid? ResolveCausationId(HttpContext context)
        {
            var header = context.Request.Headers["x-causation-id"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) && parsed != Guid.Empty
                ? parsed
                : null;
        }
    }
}