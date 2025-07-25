using EventDispatcher.Contracts;

namespace EventDispatcher.Serialization.Interface
{
    public interface IEventDeserializer
    {
        IEvent Deserialize(string payload, int schemaVersion);
    }
}

