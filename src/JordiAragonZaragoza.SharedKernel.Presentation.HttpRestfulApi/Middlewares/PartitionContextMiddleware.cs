namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Context.Partition;
    using Microsoft.AspNetCore.Http;

    public sealed class PartitionContextMiddleware
    {
        private readonly RequestDelegate next;

        public PartitionContextMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IPartitionContextService partitionContextService)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(partitionContextService);

            string? tenantId = context.Request.Headers[PartitionConstants.TenantId];
            string? partitionId = context.Request.Headers[PartitionConstants.PartitionId];

            if (tenantId is null || partitionId is null)
            {
                if (AnonymousRequestHelper.IsAnonymousAllowed(context))
                {
                    tenantId = PartitionConstants.AnonymousTenant;
                    partitionId = PartitionConstants.AnonymousPartition;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Missing TenantId or PartitionId headers.");

                    return;
                }
            }

            partitionContextService.SetPartitionContext(tenantId!, partitionId!);

            await this.next(context);
        }
    }
}