using System;
using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQueryHandler
    {
        void Handle(DomainEvent domainEvent);
        IEnumerable<Type> SubscribedTypes { get; }
    }

    public abstract class QueryHandler<T> : IQueryHandler where T : Query
    {
        public readonly SubscribedEventTypes<T> SubscribedEventTypes;
        public readonly T QueryObject;

        public QueryHandler(T queryObject, SubscribedEventTypes<T> subscribedEventTypes)
        {
            QueryObject = queryObject;
            SubscribedEventTypes = subscribedEventTypes;
        }

        public void Handle(DomainEvent domainEvent)
        {
            if (SubscribedEventTypes.Contains(domainEvent.GetType()))
            {
                QueryObject.Apply(domainEvent);
            }
        }

        public IEnumerable<Type> SubscribedTypes => SubscribedEventTypes;
    }
}