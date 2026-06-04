namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization
{
    using System;
    using System.Text;
    using global::KurrentDB.Client;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Events;

    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore;
    using Newtonsoft.Json;

    public static class SerializerHelper
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new NonDefaultConstructorContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static EventData Serialize(IDomainEvent @event, ExecutionContext? executionContext)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var metadata = executionContext is not null
                ? EventStoreMetadata.From(executionContext, @event.DateOccurredOnUtc)
                : null;

            return new EventData(
                eventId: Uuid.FromGuid(@event.Id),
                type: EventTypeMapper.Instance.ToName(@event.GetType()),
                data: Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(@event, SerializerSettings)),
                metadata: Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(metadata ?? (object)new { }, SerializerSettings)));
        }

        public static (IDomainEvent Event, EventStoreMetadata? Metadata) Deserialize(
            ResolvedEvent resolvedEvent)
        {
            var dataType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);

            var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var domainEvent = JsonConvert.DeserializeObject(data, dataType, SerializerSettings)
                ?? throw new InvalidOperationException(
                    $"Deserialization failed for event type '{resolvedEvent.Event.EventType}'.");

            var metadataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span);
            var eventMetadata = DeserializeMetadata(metadataJson);

            if (eventMetadata is not null && domainEvent is BaseDomainEvent baseDomainEvent)
            {
                domainEvent = baseDomainEvent with { DateOccurredOnUtc = eventMetadata.DateOccurredOnUtc };
            }

            return ((IDomainEvent)domainEvent, eventMetadata);
        }

        private static EventStoreMetadata? DeserializeMetadata(string metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson) || metadataJson == "{}")
            {
                return null;
            }

            try
            {
                var metadata = JsonConvert.DeserializeObject<EventStoreMetadata>(
                    metadataJson, SerializerSettings);

                // If required fields are empty, it's either native KurrentDB/Aspire metadata ($traceId/$spanId)
                // or a legacy event without context.
                return metadata?.IsValid() == true ? metadata : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}