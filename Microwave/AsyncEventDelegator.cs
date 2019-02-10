using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Queries;

namespace Microwave
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IAsyncEventHandler> _asyncEventHandlers;
        private readonly IEnumerable<IQueryEventHandler> _queryHandlers;
        private readonly IEnumerable<IReadModelHandler> _readModelHandlers;

        public AsyncEventDelegator(
            IEnumerable<IAsyncEventHandler> asyncEventHandlers,
            IEnumerable<IQueryEventHandler> queryHandlers,
            IEnumerable<IReadModelHandler> readModelHandlers)
        {
            _asyncEventHandlers = asyncEventHandlers;
            _queryHandlers = queryHandlers;
            _readModelHandlers = readModelHandlers;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);

                foreach (var handler in _queryHandlers) await SecureCall(() => handler.Update());
                foreach (var handler in _readModelHandlers) await SecureCall(() => handler.Update());
                foreach (var handler in _asyncEventHandlers) await SecureCall(() => handler.Update());
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