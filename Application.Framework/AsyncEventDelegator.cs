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
            foreach (var handler in _handler) await handler.Update();
        }
    }
}