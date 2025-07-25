namespace EventDispatcher.Contracts
{
    public interface IVersionedEvent : IEvent
    {
        int Version { get; }
    }
}
