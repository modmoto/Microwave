using Domain.Framework;

namespace Application.Framework
{
    public abstract class QueryHandler<T> where T : Query
    {
        protected readonly SubscribedEventTypes<T> SubscribedEventTypes;
        protected readonly T QueryObject;

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
    }
}