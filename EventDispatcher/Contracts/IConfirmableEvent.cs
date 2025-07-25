using EventDispatcher.Model;
using System.Threading.Tasks;

namespace EventDispatcher.Contracts
{
    public interface IConfirmableEvent : IEvent
    {
        TaskCompletionSource<HandlerResult> CompletionSource { get; }
    }
    
}
