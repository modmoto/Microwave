using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Framework
{
    public class AsyncEventDelegator
    {
        private readonly IEnumerable<IEventDelegateHandler> _handler;

        public AsyncEventDelegator(IEnumerable<IEventDelegateHandler> handler)
        {
            _handler = handler;
        }

        public async Task Update()
        {
            while (true)
            {
                await Task.Delay(1000);
                Console.WriteLine("GetNewEvents");
                //foreach (var handler in _handler) await handler.Update();
            }
        }
    }
}