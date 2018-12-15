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

                try
                {
                    foreach (var handler in _handler) await handler.Update();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString(), ConsoleColor.Red);
                }

                try
                {
                    foreach (var handler in _queryEventHandlers) await handler.Update();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString(), ConsoleColor.Red);
                }

                try
                {
                    foreach (var handler in _identifiableQueryEventHandlers) await handler.Update();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString(), ConsoleColor.Red);
                }
            }
        }
    }
}