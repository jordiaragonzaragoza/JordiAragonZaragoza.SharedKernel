namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Serialization
{
    using System;
    using System.Text;
    using global::KurrentDB.Client;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using Newtonsoft.Json;

    public static class SerializerHelper
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new NonDefaultConstructorContractResolver(),
            Converters = { new EventStoreDBEventMetadataJsonConverter() },
        };

        public static EventData Serialize(IDomainEvent @event, object? metadata = null)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return new EventData(
                eventId: Uuid.FromGuid(@event.Id),
                type: EventTypeMapper.Instance.ToName(@event.GetType()),
                data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, SerializerSettings)),
                metadata: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new { }, SerializerSettings)));
        }

        public static IDomainEvent Deserialize(ResolvedEvent resolvedEvent)
        {
            var dataType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);

            var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var domainEvent = JsonConvert.DeserializeObject(data, dataType, SerializerSettings)
                ?? throw new InvalidOperationException($"Deserialization failed for event type '{resolvedEvent.Event.EventType}'.");

            return (IDomainEvent)domainEvent;
        }
    }
}