namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using System;
    using System.Linq;
    using Asp.Versioning.ApiExplorer;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Middlewares;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class HttpRestfulApiApplicationExtensions
    {
        public static WebApplication UseWebApplicationConfigurations(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            var apiOptions = app.Services.GetRequiredService<ApiOptions>();
            var swaggerOAuth2 = app.Services.GetRequiredService<SwaggerOAuth2Options>();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            if (apiOptions.EnableCors)
            {
                app.UseSharedKernelCors();
            }

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<ExecutionContextMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var group in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse().Select(description => description.GroupName))
                    {
                        options.SwaggerEndpoint($"/swagger/{group}/swagger.json", group.ToUpperInvariant());
                    }

                    options.OAuthClientId(swaggerOAuth2.ClientId);
                    options.OAuthUsePkce();
                    options.OAuthScopes(swaggerOAuth2.EffectiveScopes.ToArray());
                });
            }

            app.MapControllers();

            return app;
        }
    }
}
