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

        public AsyncEventDelegator(
            IEnumerable<IEventDelegateHandler> handler,
            IProjectionHandler projectionHandler,
            ITypeProjectionHandler typeProjectionHandler)
        {
            _handler = handler;
            _projectionHandler = projectionHandler;
            _typeProjectionHandler = typeProjectionHandler;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);
                await _projectionHandler.Update();
                await _typeProjectionHandler.Update();

                Console.WriteLine("GetNewEvents");
                //foreach (var handler in _handler) await handler.Update();
            }
        }
    }
}