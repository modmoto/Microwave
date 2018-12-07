using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions
{
    // TODO remove this hack with actors or something (Task will break on exceptions)
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IEventDelegateHandler> _handler;
        private readonly IEnumerable<IQueryEventHandler> _queryEventHandlers;
        private readonly IEnumerable<IReadModelHandler> _identifiableQueryEventHandlers;
        private object _lock = new object();

        public AsyncEventDelegator(
            IEnumerable<IEventDelegateHandler> handler,
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

                foreach (var handler in _handler) await handler.Update();
                foreach (var handler in _queryEventHandlers) await handler.Update();
                foreach (var handler in _identifiableQueryEventHandlers)
                {
                    // Todo make lock with a eventque
                    lock (_lock)
                    {
                        handler.Update().Wait();
                    }
                }
            }
        }
    }
}