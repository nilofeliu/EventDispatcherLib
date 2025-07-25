// EventDispatcher/Resilience/IICircuitBreaker.cs
using EventDispatcher.Model;
using System;
using System.Threading.Tasks;

public interface ICircuitBreaker
{
    bool IsTripped { get; }
    Task<HandlerResult> ExecuteAsync(Func<Task<HandlerResult>> action);
    void Reset();
}
