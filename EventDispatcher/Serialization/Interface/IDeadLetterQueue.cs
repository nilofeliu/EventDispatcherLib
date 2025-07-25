using EventDispatcher.Serialization.Envelope;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventDispatcher.Serialization.Interface
{
    public interface IDeadLetterQueue
    {
        Task SaveAsync(SerializedEventEnvelope envelope, string reason);
        IEnumerable<SerializedEventEnvelope> GetFailedEvents();
    }
}

