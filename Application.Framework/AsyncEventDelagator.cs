using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class AsyncEventDelagator<T> : IHandleAsync<T> where T : DomainEvent
    {
        private readonly IEnumerable<IHandleAsync<T>>  _handleAsync;

        public AsyncEventDelagator(IEnumerable<IHandleAsync<T>> handleAsync)
        {
            _handleAsync = handleAsync;
        }

        public async Task Handle(T domainEvent)
        {
            var handleTasks = _handleAsync.Select(handler => handler.Handle(domainEvent));
            await Task.WhenAll(handleTasks);
        }
    }

    public class EventDelagator<T> : IHandle<T> where T : DomainEvent
    {
        private readonly IEnumerable<IHandle<T>>  _handleAsync;

        public EventDelagator(IEnumerable<IHandle<T>> handleAsync)
        {
            _handleAsync = handleAsync;
        }

        public void Handle(T domainEvent)
        {
            foreach (var handler in _handleAsync)
            {
                handler.Handle(domainEvent);
            }
        }
    }
}