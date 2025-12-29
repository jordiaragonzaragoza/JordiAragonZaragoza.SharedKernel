namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb
{
    public sealed class KurrentDbOptions
    {
        public const string Section = "EventStore";

        public string ConnectionString { get; set; } = default!;
    }
}