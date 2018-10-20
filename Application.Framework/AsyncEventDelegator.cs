using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class AsyncEventDelegator<T> where T : DomainEvent
    {
        private readonly IEnumerable<IHandleAsync<T>>  _handleAsync;

        public AsyncEventDelegator(IEnumerable<IHandleAsync<T>> handleAsync)
        {
            _handleAsync = handleAsync;
        }

        public async Task Handle(T domainEvent)
        {
            var handleTasks = _handleAsync.Select(handler => handler.Handle(domainEvent));
            await Task.WhenAll(handleTasks);
        }
    }
}