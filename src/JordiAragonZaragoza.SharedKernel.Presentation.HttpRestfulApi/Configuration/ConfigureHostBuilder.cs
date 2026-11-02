namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class ConfigureHostBuilder
    {
        public static IHostBuilder UseHostBuilderConfigurations(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            hostBuilder.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);

            return hostBuilder;
        }
    }
}