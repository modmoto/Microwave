using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryEventHandler<T> : IEventHandler where T : Query
    {
        public T QueryObject { get; }
        public IEnumerable<Type> SubscribedDomainEventTypes { get; }

        public QueryEventHandler(T queryObject, SubscribedEventTypes<T> subscribedEventTypes)
        {
            QueryObject = queryObject;
            SubscribedDomainEventTypes = subscribedEventTypes;
        }

        public void Handle(DomainEvent domainEvent)
        {
            QueryObject.Apply(domainEvent);
        }
    }
}