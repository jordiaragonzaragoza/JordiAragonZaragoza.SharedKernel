namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031: Do not catch general exception types", Justification = "Main exception handler for http requests")]
        public async Task InvokeAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            try
            {
                await this.next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorDetails = new
            {
                context.Response.StatusCode,
                exception.Message,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorDetails), CancellationToken.None);
        }
    }
}