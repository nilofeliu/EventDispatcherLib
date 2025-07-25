using EventDispatcher.Contracts;
using EventDispatcher.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventDispatcher.Middleware
{
    public class ConsoleLoggingMiddleware : IEventMiddleware
    {
        public async Task<HandlerResult> InvokeAsync(IEvent evt, CancellationToken token, Func<Task<HandlerResult>> next)
        {
            Console.WriteLine($"[Middleware] Handling {evt.GetType().Name} (ID: {evt.Id})");
            var result = await next();
            Console.WriteLine($"[Middleware] Result: {(result.Success ? "Success" : "Failure")}");
            return result;
        }
    }

}
