namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.AspNetCore.Http;

    public sealed class UserContextMiddleware
    {
        private readonly RequestDelegate next;

        public UserContextMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IUserContextService userContextService)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(userContextService);

            string? userId = context.Request.Headers[UserConstants.UserId];
            ////context.User.FindFirstValue(ClaimTypes.NameIdentifier)

            if (userId is null)
            {
                if (AnonymousRequestHelper.IsAnonymousAllowed(context))
                {
                    userId = UserConstants.AnonymousUser;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Missing UserId headers.");

                    return;
                }
            }

            userContextService.SetUserContext(userId!);

            await this.next(context);
        }
    }
}