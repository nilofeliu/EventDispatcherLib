//using EventDispatcher.Contracts;
//using EventDispatcher.Model;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace EventDispatcher.Core
//{
//    public class EventHandlerRegistry
//    {
//        private readonly ConcurrentDictionary<Type, List<Func<IEvent, CancellationToken, Task<HandlerResult>>>> _handlers = new();

//        public void Register<TEvent>(IEventHandler<TEvent> handler, params IEventMiddleware[] middlewares) where TEvent : IEvent
//        {
//            var key = typeof(TEvent);

//            var wrappedHandler = new Func<IEvent, CancellationToken, Task<HandlerResult>>(async (evt, token) =>
//            {
//                Func<Task<HandlerResult>> finalHandler = () => handler.HandleAsync((TEvent)evt, token);

//                foreach (var middleware in middlewares.Reverse())
//                {
//                    var next = finalHandler;
//                    finalHandler = () => middleware.InvokeAsync(evt, token, next);
//                }

//                return await finalHandler();
//            });

//            _handlers.AddOrUpdate(key,
//                _ => new List<Func<IEvent, CancellationToken, Task<HandlerResult>>> { wrappedHandler },
//                (_, list) => { list.Add(wrappedHandler); return list; });
//        }

//        public IEnumerable<Func<IEvent, CancellationToken, Task<HandlerResult>>> GetHandlers(Type eventType) =>
//            _handlers.TryGetValue(eventType, out var list) ? list : Enumerable.Empty<Func<IEvent, CancellationToken, Task<HandlerResult>>>();
//    }

//}

using EventDispatcher.Contracts;
using EventDispatcher.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Core
{
    /// <summary>
    /// Holds registered event handlers and composes them with optional middleware.
    /// </summary>
    public class EventHandlerRegistry
    {
        private readonly ConcurrentDictionary<Type, List<Func<IEvent, CancellationToken, Task<HandlerResult>>>> _handlers = new();

        /// <summary>
        /// Registers a handler for the specified event type, with optional middleware.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="handler">Concrete handler instance.</param>
        /// <param name="middlewares">Middleware to wrap around the handler.</param>
        /// <returns>The registry itself (for fluent chaining).</returns>
        public EventHandlerRegistry Register<TEvent>(
            IEventHandler<TEvent> handler,
            params IEventMiddleware[] middlewares
        ) where TEvent : IEvent
        {
            var key = typeof(TEvent);

            var wrappedHandler = new Func<IEvent, CancellationToken, Task<HandlerResult>>(async (evt, token) =>
            {
                Func<Task<HandlerResult>> finalHandler = () => handler.HandleAsync((TEvent)evt, token);

                foreach (var middleware in middlewares.Reverse())
                {
                    var next = finalHandler;
                    finalHandler = () => middleware.InvokeAsync(evt, token, next);
                }

                return await finalHandler();
            });

            _handlers.AddOrUpdate(key,
                _ => new List<Func<IEvent, CancellationToken, Task<HandlerResult>>> { wrappedHandler },
                (_, list) => { list.Add(wrappedHandler); return list; });

            return this;
        }

        /// <summary>
        /// Retrieves all handlers registered for the given event type.
        /// </summary>
        public IEnumerable<Func<IEvent, CancellationToken, Task<HandlerResult>>> GetHandlers(Type eventType)
            => _handlers.TryGetValue(eventType, out var list)
                ? list
                : Enumerable.Empty<Func<IEvent, CancellationToken, Task<HandlerResult>>>();
    }
}
