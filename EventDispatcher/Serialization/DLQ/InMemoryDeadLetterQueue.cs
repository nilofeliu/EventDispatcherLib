using EventDispatcher.Serialization.Envelope;
using EventDispatcher.Serialization.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventDispatcher.Serialization.DLQ
{
    public class InMemoryDeadLetterQueue : IDeadLetterQueue
    {
        private readonly int _maxCapacity;
        private readonly ConcurrentQueue<SerializedEventEnvelope> _deadLetters = new();

        public InMemoryDeadLetterQueue(int maxCapacity = 1000)
        {
            _maxCapacity = maxCapacity;
        }

        public Task SaveAsync(SerializedEventEnvelope envelope, string reason)
        {
            // 🔐 Ensure capacity control
            if (_deadLetters.Count >= _maxCapacity)
            {
                _deadLetters.TryDequeue(out _); // 🚪 Evict oldest
            }

            _deadLetters.Enqueue(envelope);
            Console.WriteLine($"📥 DLQ Stored: {envelope.EventType} – Reason: {reason}");

            return Task.CompletedTask;
        }

        public int Count => _deadLetters.Count;

        public IEnumerable<SerializedEventEnvelope> GetFailedEvents()
        {
            return _deadLetters.ToArray(); // Snapshot for safe enumeration
        }
    }
}