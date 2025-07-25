using System;

namespace EventDispatcher.Contracts
{
    public interface IEvent
    {
        Guid Id { get; }
        DateTime CreatedAt { get; }
    }
    
}
