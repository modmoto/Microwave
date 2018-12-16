using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions
{
    // TODO remove this hack with actors or something (Task will break on exceptions)
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

                foreach (var handler in _handler) SecureCall(async () => await handler.Update());
                foreach (var handler in _queryEventHandlers) SecureCall(async () => await handler.Update());
                foreach (var handler in _identifiableQueryEventHandlers) SecureCall(async () => await handler.Update());
            }
        }

        private void SecureCall(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                var currentForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.ToString(), ConsoleColor.Red);
                Console.ForegroundColor = currentForeground;
            }
        }
    }
}