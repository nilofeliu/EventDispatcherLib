using EventDispatcher.Model;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Contracts
{
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task<HandlerResult> HandleAsync(TEvent evt, CancellationToken token);
    }
    
}
