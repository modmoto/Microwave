using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Framework
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IEventDelegateHandler> _handler;
        private readonly IProjectionHandler _projectionHandler;
        private readonly ITypeProjectionHandler _typeProjectionHandler;
        private readonly IEnumerable<IQueryEventHandler> _queryEventHandlers;

        public AsyncEventDelegator(
            IEnumerable<IEventDelegateHandler> handler,
            IProjectionHandler projectionHandler,
            ITypeProjectionHandler typeProjectionHandler,
            IEnumerable<IQueryEventHandler> queryEventHandlers)
        {
            _handler = handler;
            _projectionHandler = projectionHandler;
            _typeProjectionHandler = typeProjectionHandler;
            _queryEventHandlers = queryEventHandlers;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);
                await _projectionHandler.Update();
                await _typeProjectionHandler.Update();

                foreach (var handler in _handler) await handler.Update();
                foreach (var handler in _queryEventHandlers) await handler.Update();
            }
        }
    }
}