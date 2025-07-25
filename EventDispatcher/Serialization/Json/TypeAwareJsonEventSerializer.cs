// EventDispatcher/Serialization/TypeAwareJsonEventSerializer.cs
using EventDispatcher.Contracts;
using EventDispatcher.Serialization.Interface;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventDispatcher.Serialization.Json
{
    /// <summary>
    /// Enhanced JSON serializer that uses EventTypeRegistry for robust type resolution.
    /// </summary>
    public class TypeAwareJsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;
        private readonly EventTypeRegistry _typeRegistry;

        public TypeAwareJsonEventSerializer(
            EventTypeRegistry typeRegistry = null,
            JsonSerializerOptions options = null)
        {
            _typeRegistry = typeRegistry ?? new EventTypeRegistry();
            _options = options ?? new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public EventTypeRegistry TypeRegistry => _typeRegistry;

        public string Serialize(IEvent eventObj)
        {
            if (eventObj == null)
                throw new ArgumentNullException(nameof(eventObj));

            return JsonSerializer.Serialize(eventObj, eventObj.GetType(), _options);
        }

        public IEvent Deserialize(string payload, Type eventType)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            try
            {
                var obj = JsonSerializer.Deserialize(payload, eventType, _options);
                return obj as IEvent ?? throw new InvalidOperationException(
                    $"Deserialized object is not an IEvent: {eventType.Name}");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize JSON payload into {eventType.Name}", ex);
            }
        }

        /// <summary>
        /// Deserializes using type name resolution through the registry.
        /// </summary>
        public IEvent Deserialize(string payload, string typeName)
        {
            var eventType = _typeRegistry.ResolveType(typeName);
            return Deserialize(payload, eventType);
        }
    }
}

