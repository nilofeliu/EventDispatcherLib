using System.Threading;

namespace EventDispatcher.Core
{
    public class EventMetrics
    {
        private long _eventsProcessed;
        private long _eventsFailed;
        private long _eventsRetried;
        private long _eventsDropped;

        public long EventsProcessed => _eventsProcessed;
        public long EventsFailed => _eventsFailed;
        public long EventsRetried => _eventsRetried;
        public long EventsDropped => _eventsDropped;

        public void IncrementProcessed() => Interlocked.Increment(ref _eventsProcessed);
        public void IncrementFailed() => Interlocked.Increment(ref _eventsFailed);
        public void IncrementRetried() => Interlocked.Increment(ref _eventsRetried);
        public void IncrementDropped() => Interlocked.Increment(ref _eventsDropped);

        public void Reset()
        {
            Interlocked.Exchange(ref _eventsProcessed, 0);
            Interlocked.Exchange(ref _eventsFailed, 0);
            Interlocked.Exchange(ref _eventsRetried, 0);
            Interlocked.Exchange(ref _eventsDropped, 0);
        }
    }
    
}
