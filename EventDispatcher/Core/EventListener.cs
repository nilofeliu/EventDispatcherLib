using EventDispatcher.Configuration;
using EventDispatcher.Contracts;
using EventDispatcher.Host;
using EventDispatcher.Model;
using EventDispatcher.Serialization.Envelope;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Core
{
    public class EventListener : IDisposable
    {
        private readonly ConcurrentQueue<IConfirmableEvent> _priorityQueue = new();
        private readonly ConcurrentQueue<IEvent> _eventQueue = new();

        private readonly SemaphoreSlim _signal = new(0);
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _listenerTask;
        private readonly EventHandlerRegistry _registry;
        private readonly ConcurrentDictionary<Guid, int> _retryTracker = new();
        private readonly EventListenerConfig _config;
        private readonly EventMetrics _metrics;
        private readonly ICircuitBreaker _circuitBreaker;

        public EventMetrics Metrics => _metrics;

        public EventListener(EventHandlerRegistry registry, EventListenerConfig config = null)
        {
            _registry = registry;
            _config = config ?? new EventListenerConfig();
            _metrics = new EventMetrics();
            _listenerTask = Task.Run(ListenLoopAsync);
            _circuitBreaker = new CircuitBreaker(config?.CircuitBreakerFailureThreshold ?? 5,
        config?.CircuitBreakerResetTimeout ?? TimeSpan.FromMinutes(1));
        }

        public void Enqueue(IEvent evt)
        {
            if (evt is IConfirmableEvent confirmable)
                _priorityQueue.Enqueue(confirmable);
            else
                _eventQueue.Enqueue(evt);

            _signal.Release();
        }

        public async Task<HandlerResult> EnqueueAsync(IConfirmableEvent evt, CancellationToken token = default)
        {
            _priorityQueue.Enqueue(evt);
            _signal.Release();

            using var combined = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            try
            {
                return await evt.CompletionSource.Task.WaitAsync(combined.Token);
            }
            catch (OperationCanceledException)
            {
                return HandlerResult.Fail("Operation was cancelled");
            }
        }

        private async Task ListenLoopAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await _signal.WaitAsync(_cts.Token);

                IEvent evt = null;

                // 🔼 Priority: confirmable events come first
                if (_priorityQueue.TryDequeue(out var priorityEvt))
                    evt = priorityEvt;
                else if (_eventQueue.TryDequeue(out var normalEvt))
                    evt = normalEvt;

                if (evt == null) continue;

                var handlers = _registry.GetHandlers(evt.GetType()).ToList();
                if (handlers.Count == 0)
                {
                    Console.WriteLine($"⚠️ No handlers for {evt.GetType().Name}");
                    CompleteConfirmable(evt, HandlerResult.Fail("No handlers found"));
                    continue;
                }

                bool success = await ProcessEventAsync(evt, handlers);
                var result = success ? HandlerResult.Ok() : HandlerResult.Fail("Handler execution failed");

                if (success)
                {
                    _metrics.IncrementProcessed();
                    _retryTracker.TryRemove(evt.Id, out _);
                    CompleteConfirmable(evt, result);
                }
                else
                {
                    bool retrying = await HandleEventFailureAsync(evt);
                    if (!retrying)
                        CompleteConfirmable(evt, result);
                }
            }
        }


        private void CompleteConfirmable(IEvent evt, HandlerResult result)
        {
            if (evt is IConfirmableEvent confirmable)
                confirmable.CompletionSource.TrySetResult(result);
        }

        private async Task<bool> ProcessEventAsync(IEvent evt, List<Func<IEvent, CancellationToken, Task<HandlerResult>>> handlers)
        {
            bool success = true;

            foreach (var handler in handlers)
            {
                try
                {
                    var result = await handler(evt, _cts.Token);
                    if (!result.Success)
                    {
                        Console.WriteLine($"❌ Handler failed: {result.Message}");
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Exception: {ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        private async Task<bool> HandleEventFailureAsync(IEvent evt)
        {
            _metrics.IncrementFailed();

            var retryCount = _retryTracker.GetOrAdd(evt.Id, 0);
            if (retryCount < _config.MaxRetries)
            {
                retryCount++;
                _retryTracker[evt.Id] = retryCount;

                int backoff = (int)Math.Pow(_config.BackoffMultiplier, retryCount) * _config.BaseBackoffMs;
                await Task.Delay(backoff, _cts.Token);

                // 🔁 Re-enqueue into appropriate queue
                if (evt is IConfirmableEvent confirmable)
                    _priorityQueue.Enqueue(confirmable);
                else
                    _eventQueue.Enqueue(evt);

                _signal.Release();
                _metrics.IncrementRetried();

                Console.WriteLine($"🔁 Retrying event ({retryCount}/{_config.MaxRetries})");
                return true;
            }
            else
            {
                _metrics.IncrementDropped();
                _retryTracker.TryRemove(evt.Id, out _);
                Console.WriteLine($"⛔ Event dropped after max retries: {evt.Id}");

                // 📨 Serialize and store in DLQ
                var envelope = new SerializedEventEnvelope
                {
                    EventType = evt.GetType().FullName,
                    Payload = EventDispatcherHost.Serializer.Serialize(evt),
                    Timestamp = DateTime.UtcNow
                };

                await EventDispatcherHost.DeadLetterQueue.SaveAsync(envelope, "Max retries exceeded");



                return false;
            }
        }


        public void Stop() => _cts.Cancel();

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _signal?.Dispose();
        }
    }
    
}
