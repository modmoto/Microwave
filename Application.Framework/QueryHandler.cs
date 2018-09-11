using System;
using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public class QueryHandler<T> : IQueryHandler where T : Query
    {
        public T QueryObject { get; private set; }

        public QueryHandler(T queryObject, SubscribedEventTypes<T> subscribedEventTypes)
        {
            QueryObject = queryObject;
            SubscribedTypes = subscribedEventTypes;
        }

        public void Handle(DomainEvent domainEvent)
        {
            QueryObject.Apply(domainEvent);
        }

        public IEnumerable<Type> SubscribedTypes { get; }
        public Type HandledQuery => typeof(T);

        public void SetObject(Query snapShotQuerry)
        {
            QueryObject = (T) snapShotQuerry;
        }
    }
}