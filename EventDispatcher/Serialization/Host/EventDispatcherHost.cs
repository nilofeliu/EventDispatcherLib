// Example Usage and Extension to EventDispatcherHost
using EventDispatcher.Configuration;
using EventDispatcher.Core;
using EventDispatcher.Serialization;
using EventDispatcher.Serialization.DLQ;
using EventDispatcher.Serialization.Interface;
using EventDispatcher.Serialization.Json;
using System;

namespace EventDispatcher.Host
{
    public static partial class EventDispatcherHost
    {
        /// <summary>
        /// Initializes with enhanced type-aware serialization.
        /// </summary>
        public static void InitializeWithTypeRegistry(
            EventListenerConfig config = null,
            EventTypeRegistry typeRegistry = null,
            IDeadLetterQueue deadLetterQueue = null)
        {
            var registry = typeRegistry ?? new EventTypeRegistry();

            // Auto-register event types from calling assembly
            registry.AutoRegisterFromAssemblies();

            Registry = new EventHandlerRegistry();
            Serializer = new TypeAwareJsonEventSerializer(registry);
            Listener = new EventListener(Registry, config ?? new EventListenerConfig());
            DeadLetterQueue = deadLetterQueue ?? new InMemoryDeadLetterQueue();
        }

        /// <summary>
        /// Gets the type registry if using TypeAwareJsonEventSerializer.
        /// </summary>
        public static EventTypeRegistry GetTypeRegistry()
        {
            return (Serializer as TypeAwareJsonEventSerializer)?.TypeRegistry
                ?? throw new InvalidOperationException("Type registry not available with current serializer");
        }
    }
}