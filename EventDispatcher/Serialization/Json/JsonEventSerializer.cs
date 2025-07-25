using EventDispatcher.Contracts;
using EventDispatcher.Serialization.Interface;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace EventDispatcher.Serialization.Json
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonEventSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public string Serialize(IEvent eventObj)
        {
            return JsonSerializer.Serialize(eventObj, eventObj.GetType(), _options);
        }

        public IEvent Deserialize(string payload, Type eventType)
        {
            var obj = JsonSerializer.Deserialize(payload, eventType, _options);
            return obj as IEvent ?? throw new InvalidOperationException(
                $"Failed to deserialize payload into {eventType.Name}");
        }
    }
}
