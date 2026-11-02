namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using Microsoft.AspNetCore.Hosting;

    public static class ConfigureWebHostBuilder
    {
        public static IWebHostBuilder UseWebHostBuilderConfigurations(this IWebHostBuilder hostBuilder)
        {
            // Server header should not be included in each response.
            hostBuilder.UseKestrel(options => options.AddServerHeader = false);

            return hostBuilder;
        }
    }
}