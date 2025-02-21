namespace JordiAragonZaragoza.SharedKernel.Infrastructure.AssemblyConfiguration
{
    using EasyCaching.InMemory;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Cache;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Cache.EasyCaching;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class ConfigureServices
    {
        public static IServiceCollection AddSharedKernelInfrastructureServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var cacheOptions = new CacheOptions();
            configuration.Bind(CacheOptions.Section, cacheOptions);
            _ = serviceCollection.AddSingleton(Options.Create(cacheOptions));

            var easyCachingInMemoryOptions = new EasyCachingInMemoryOptions();
            configuration.Bind(EasyCachingInMemoryOptions.Section, easyCachingInMemoryOptions);
            _ = serviceCollection.AddSingleton(Options.Create(easyCachingInMemoryOptions));

            _ = serviceCollection.AddEasyCaching(options =>
            {
                _ = options.UseInMemory(
                    config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = easyCachingInMemoryOptions.DBConfigExpirationScanFrequency,

                        // total count of cache items, default value is 10000
                        SizeLimit = easyCachingInMemoryOptions.DBConfigSizeLimit,

                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = easyCachingInMemoryOptions.DBConfigEnableReadDeepClone,

                        // enable deep clone when writing object to cache or not, default valuee is false.
                        EnableWriteDeepClone = easyCachingInMemoryOptions.DBConfigEnableWriteDeepClone,
                    };

                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = easyCachingInMemoryOptions.MaxRdSecond;

                    // whether enable logging, default is false
                    config.EnableLogging = easyCachingInMemoryOptions.EnableLogging;

                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = easyCachingInMemoryOptions.LockMs;

                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = easyCachingInMemoryOptions.SleepMs;
                },
                    cacheOptions.DefaultName);
            });

            /*serviceCollection.AddEasyCaching(option =>
            {
                option.UseRedis(
                    config =>
                    {
                        config.DBConfig = new RedisDBOptions
                        {
                            // Database
                            // AsyncTimeout
                            // SyncTimeout
                            // KeyPrefix
                            // ConfigurationOptions
                        };

                        // the max random second will be added to cache's expiration, default value is 120
                        config.MaxRdSecond = easyCachingInMemoryOptions.MaxRdSecond;

                        // whether enable logging, default is false
                        config.EnableLogging = easyCachingInMemoryOptions.EnableLogging;

                        // mutex key's alive time(ms), default is 5000
                        config.LockMs = easyCachingInMemoryOptions.LockMs;

                        // when mutex key alive, it will sleep some time, default is 300
                        config.SleepMs = easyCachingInMemoryOptions.SleepMs;
                    },
                    cacheOptions.DefaultName);
            });*/

            return serviceCollection;
        }
    }
}