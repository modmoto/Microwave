using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryEventHandler<T> : IQuerryEventHandler where T : Query
    {
        private readonly object _lockObject = new Object();
        public T QueryObject { get; }
        public IEnumerable<string> SubscribedDomainEventTypes { get; }

        public QueryEventHandler(T queryObject, SubscribedEventTypes<T> subscribedEventTypes)
        {
            QueryObject = queryObject;
            SubscribedDomainEventTypes = subscribedEventTypes;
        }

        public async Task Handle(DomainEvent domainEvent)
        {
            lock (_lockObject)
            {
                QueryObject.Apply(domainEvent);
            }
        }
    }
}