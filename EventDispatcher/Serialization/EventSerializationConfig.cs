using System.Text.Json;
using System.Text.Json.Serialization;


namespace EventDispatcher.Serialization
{
    public static class EventSerializationConfig
    {
        public static JsonSerializerOptions DefaultOptions => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}