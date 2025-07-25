using EventDispatcher.Contracts;
using System;

namespace EventDispatcher.Serialization.Interface
{
    public interface IEventSerializer
    {
        string Serialize(IEvent eventObj);
        IEvent Deserialize(string payload, Type eventType);
    }
}

