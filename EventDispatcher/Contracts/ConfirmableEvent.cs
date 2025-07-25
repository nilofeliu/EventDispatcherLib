using EventDispatcher.Model;
using System;
using System.Threading.Tasks;

namespace EventDispatcher.Contracts
{
    public abstract class ConfirmableEvent : IConfirmableEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public TaskCompletionSource<HandlerResult> CompletionSource { get; } = new();
    }
    
}
