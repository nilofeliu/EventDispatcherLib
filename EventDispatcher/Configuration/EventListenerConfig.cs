using System;

namespace EventDispatcher.Configuration
{
    public class EventListenerConfig
    {
        public int MaxRetries { get; set; } = 3;
        public int BaseBackoffMs { get; set; } = 100;
        public int BackoffMultiplier { get; set; } = 2;

        // Circuit Breaker properties
        public int CircuitBreakerFailureThreshold { get; set; } = 5;  // Renamed for clarity
        public TimeSpan CircuitBreakerResetTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan HandlerTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
    
}
