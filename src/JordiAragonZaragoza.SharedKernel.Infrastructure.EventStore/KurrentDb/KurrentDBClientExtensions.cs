namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb
{
    using System.Reflection;
    using KurrentDB.Client;

    internal static class KurrentDBClientExtensions
    {
        public static KurrentDBClientSettings GetSettings(this KurrentDBClientBase client)
        {
            var prop = typeof(KurrentDBClientBase)
                .GetProperty("Settings", BindingFlags.NonPublic | BindingFlags.Instance);

            return (KurrentDBClientSettings)prop!.GetGetMethod(nonPublic: true)!.Invoke(client, null)!;
        }

        public static KurrentDBClientSettings Copy(this KurrentDBClientSettings settings)
            => new()
            {
                Interceptors = settings.Interceptors,
                ChannelCredentials = settings.ChannelCredentials,
                ConnectionName = settings.ConnectionName,
                ConnectivitySettings = settings.ConnectivitySettings,
                DefaultCredentials = settings.DefaultCredentials,
                LoggerFactory = settings.LoggerFactory,
                OperationOptions = settings.OperationOptions,
            };
    }
}