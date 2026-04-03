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

        public async Task InvokeAsync(HttpContext context, IExecutionContextService executionContextService)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(executionContextService);

            var actorIdHeader = context.Request.Headers["x-actor-id"].FirstOrDefault();
            string? actorId =
                context.User?.FindFirst("sub")?.Value ??
                actorIdHeader;

            var actorTypeHeader = context.Request.Headers["x-actor-type"].FirstOrDefault();
            string actorType =
                actorTypeHeader ??
                (context.User?.Identity?.IsAuthenticated == true
                    ? ActorConstants.User
                    : ActorConstants.Anonymous);

            var correlationIdHeader = context.Request.Headers["x-correlation-id"].FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdHeader, out var parsed)
                ? parsed
                : Guid.NewGuid();

            if (actorId is null)
            {
                if (AnonymousRequestHelper.IsAnonymousAllowed(context))
                {
                    actorId = ActorConstants.Anonymous;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("ActorId is required for this request.");

                    return;
                }
            }

            executionContextService.SetExecutionContext(actorId, actorType, correlationId, causationId: null);

            await this.next(context);

            context.Response.Headers["x-correlation-id"] = correlationId.ToString();
        }
    }
}