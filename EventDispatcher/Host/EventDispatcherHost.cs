using EventDispatcher.Configuration;
using EventDispatcher.Contracts;
using EventDispatcher.Core;
using EventDispatcher.Model;
using EventDispatcher.Serialization;
using EventDispatcher.Serialization.DLQ;
using EventDispatcher.Serialization.Interface;
using EventDispatcher.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Host
{
    public static partial class EventDispatcherHost
    {
        // Static core services
        public static EventHandlerRegistry Registry { get; private set; }
        public static EventListener Listener { get; private set; }
        public static IEventSerializer Serializer { get; private set; }
        public static IDeadLetterQueue DeadLetterQueue { get; private set; }

        /// <summary>
        /// Initializes the dispatcher and its core components.
        /// </summary>
        /// <param name="config">Optional listener configuration.</param>
        /// <param name="serializer">Optional event serializer override.</param>
        public static void Initialize(
            EventListenerConfig config = null,
            IEventSerializer serializer = null,
            IDeadLetterQueue deadLetterQueue = null)
        {
            Registry = new EventHandlerRegistry();
            Serializer = serializer ?? new JsonEventSerializer(); // Default to System.Text.Json
            Listener = new EventListener(Registry, config ?? new EventListenerConfig());
            DeadLetterQueue = deadLetterQueue ?? new InMemoryDeadLetterQueue(); // Swap with file/DB-based if needed

        }

        /// <summary>
        /// Enqueues an event for dispatch.
        /// </summary>
        public static void Enqueue(IEvent evt) => Listener.Enqueue(evt);

        /// <summary>
        /// Enqueues a confirmable event asynchronously.
        /// </summary>
        public static Task<HandlerResult> EnqueueAsync(IConfirmableEvent evt, CancellationToken token = default)
            => Listener.EnqueueAsync(evt, token);

        /// <summary>
        /// Shuts down the event listener.
        /// </summary>
        public static void Shutdown() => Listener.Stop();


    }
}
