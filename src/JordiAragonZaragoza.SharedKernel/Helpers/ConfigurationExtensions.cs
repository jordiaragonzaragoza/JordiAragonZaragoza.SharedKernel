namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static T GetRequiredConfiguration<T>(this IConfiguration configuration, string configurationKey)
        {
            return configuration.GetRequiredSection(configurationKey).Get<T>()
                   ?? throw new InvalidOperationException(
                       $"{typeof(T).Name} configuration wasn't found for '${configurationKey}' key");
        }

        public static string GetRequiredConnectionString(this IConfiguration configuration, string configurationKey)
        {
            return configuration.GetConnectionString(configurationKey)
            ?? throw new InvalidOperationException(
                $"Configuration string with name '${configurationKey}' was not found");
        }
    }
}