// EventDispatcher/Resilience/ICircuitBreakerException.cs
using System;

public partial class CircuitBreaker
{
    // Add this exception class
    public class CircuitBreakerException : Exception
    {
        public CircuitBreakerException(string message) : base(message) { }
    }
}