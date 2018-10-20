using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public class EventDelegator<T> where T : DomainEvent
    {
        private readonly IEnumerable<IHandle<T>>  _handleAsync;

        public EventDelegator(IEnumerable<IHandle<T>> handleAsync)
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