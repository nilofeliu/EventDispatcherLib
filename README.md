# EventDispatcherLib

A lightweight event dispatching system with dead-letter queue and retry policies. Zero dependencies.

## Features
- Dual Queues (Priority/Standard)
- Dead Letter Queue
- Configurable retry policies
- Thread-safe design
- No external dependencies

## Project Structure
EventDispatcher/
├── Configuration/EventListenrConfig.cs
├── Contracts/[Interfaces]
├── Core/Registry+Listener
├── Host/StaticHost.cs
├── Middleware/
├── Model/HandlerResult.cs
└── Serialization/Json+DLQ

## Basic Usage

1. Initialize:
EventDispatcherHost.Initialize();

2. Register handler:
EventDispatcherHost.Registry.Register<MyEvent>(new MyHandler());

3. Send events:
// Fire-and-forget
EventDispatcherHost.Enqueue(new MyEvent());

// Awaitable
var result = await EventDispatcherHost.EnqueueAsync(new ConfirmableEvent());

4. Check failures:
var failed = EventDispatcherHost.DeadLetterQueue.GetFailedEvents();

5. Shutdown:
EventDispatcherHost.Shutdown();

## Advanced
- Custom serializers (implement IEventSerializer)
- Scoped instances via GetScopedInstance("scope_id")

## Philosophy
- Explicit > implicit
- Zero dependencies
- Minimal API surface
- Predictable behavior

License: MIT
