namespace JordiAragonZaragoza.SharedKernel.Infrastructure.AssemblyConfiguration
{
    using Microsoft.Extensions.Configuration;

    public static class Configuration
    {
        public static IConfigurationBuilder AddSharedKernelInfrastructureDefaultConfiguration(this IConfigurationBuilder configurationBuilder, string environmentName)
        {
            var assemblyName = InfrastructureAssemblyReference.Assembly.GetName().Name;
            var resourcePath = $"{assemblyName}.sharedkernelinfrastructuresettings.{environmentName}.json";

            using var stream = InfrastructureAssemblyReference.Assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                _ = configurationBuilder.AddJsonStream(stream);
            }

            return configurationBuilder;
        }
    }
}