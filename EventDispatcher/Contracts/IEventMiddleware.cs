using EventDispatcher.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Contracts
{
    public interface IEventMiddleware
    {
        Task<HandlerResult> InvokeAsync(IEvent evt, CancellationToken token, Func<Task<HandlerResult>> next);
    }
}
