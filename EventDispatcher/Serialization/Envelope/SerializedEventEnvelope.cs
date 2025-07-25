// EventDispatcher/Serialization/SerializedEventEnvelope.cs (Updated)
using EventDispatcher.Contracts;
using EventDispatcher.Serialization.Interface;
using EventDispatcher.Serialization.Json;
using System;

namespace EventDispatcher.Serialization.Envelope
{
    public class SerializedEventEnvelope
    {
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; }
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// Creates an envelope from an event using the provided serializer.
        /// </summary>
        public static SerializedEventEnvelope Create(IEvent evt, IEventSerializer serializer)
        {
            var typeName = evt.GetType().FullName;

            // If using TypeAwareJsonEventSerializer, get the registered name
            if (serializer is TypeAwareJsonEventSerializer typeAwareSerializer)
            {
                typeName = typeAwareSerializer.TypeRegistry.GetTypeName(evt.GetType());
            }

            return new SerializedEventEnvelope
            {
                EventType = typeName,
                Payload = serializer.Serialize(evt),
                Timestamp = DateTime.UtcNow,
                SchemaVersion = 1
            };
        }

        /// <summary>
        /// Deserializes the envelope back to an event using the provided serializer.
        /// </summary>
        public IEvent Deserialize(IEventSerializer serializer)
        {
            if (serializer is TypeAwareJsonEventSerializer typeAwareSerializer)
            {
                return typeAwareSerializer.Deserialize(Payload, EventType);
            }
            else
            {
                // Fallback to Type.GetType for other serializers
                var eventType = Type.GetType(EventType, throwOnError: true);
                return serializer.Deserialize(Payload, eventType);
            }
        }
    }
}
