// EventDispatcher/Resilience/ICircuitBreaker.cs
using EventDispatcher.Model;
using System;
using System.Threading.Tasks;

// EventDispatcher/Resilience/CircuitBreaker.cs
public partial class CircuitBreaker : ICircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;
    private int _failureCount;
    private DateTime _lastTripTime;
    private bool _isTripped;

    public bool IsTripped => _isTripped &&
                           (DateTime.UtcNow - _lastTripTime) < _resetTimeout;

    public CircuitBreaker(int failureThreshold, TimeSpan resetTimeout)
    {
        _failureThreshold = failureThreshold;
        _resetTimeout = resetTimeout;
    }

    // EventDispatcher/Resilience/CircuitBreaker.cs

    // ... existing fields and constructor ...

    public async Task<HandlerResult> ExecuteAsync(Func<Task<HandlerResult>> action)
    {
        if (IsTripped)
        {
            return HandlerResult.Fail("Circuit breaker tripped",
                                    new CircuitBreakerException("Circuit is open"));
        }

        try
        {
            var result = await action();
            if (!result.Success)
            {
                RecordFailure();
            }
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure();
            return HandlerResult.Fail("Operation failed in circuit breaker", ex);
        }
    }

    private void RecordFailure()
    {
        _failureCount++;
        if (_failureCount >= _failureThreshold)
        {
            _isTripped = true;
            _lastTripTime = DateTime.UtcNow;
        }
    }

    public void Reset()
    {
        _isTripped = false;
        _failureCount = 0;
    }
}