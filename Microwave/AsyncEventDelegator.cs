using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Queries;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _handler;
        private readonly IEnumerable<IQueryEventHandler> _queryEventHandlers;
        private readonly IEnumerable<IReadModelHandler> _identifiableQueryEventHandlers;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> handler,
            IEnumerable<IQueryEventHandler> queryEventHandlers,
            IEnumerable<IReadModelHandler> identifiableQueryEventHandlers)
        {
            _handler = handler;
            _queryEventHandlers = queryEventHandlers;
            _identifiableQueryEventHandlers = identifiableQueryEventHandlers;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);

                foreach (var handler in _handler) await SecureCall(() => handler.Update());
                foreach (var handler in _queryEventHandlers) await SecureCall(() => handler.Update());
                foreach (var handler in _identifiableQueryEventHandlers) await SecureCall(() => handler.Update());
            }
        }

        private async Task SecureCall(Func<Task> action)
        {
            try
            {
                await action.Invoke();
            }
            catch (Exception e)
            {
                var currentForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Exception was thrown during a Async Handler, this queue is stuck now");
                Console.Error.WriteLine(e.ToString());
                Console.ForegroundColor = currentForeground;
            }
        }
    }
}