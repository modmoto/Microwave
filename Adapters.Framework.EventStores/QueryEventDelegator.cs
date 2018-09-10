using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEventStoreConnection _connection;
        private readonly IDomainEventConverter _domainEventConverter;
        private readonly EventStoreConfig _eventStoreConfig;
        private readonly IEnumerable<IQueryHandler> _handlerList;

        public QueryEventDelegator(IEnumerable<IQueryHandler> handlerList, IEventStoreConnection connection,
            EventStoreConfig eventStoreConfig, IDomainEventConverter domainEventConverter)
        {
            _handlerList = handlerList;
            _connection = connection;
            _eventStoreConfig = eventStoreConfig;
            _domainEventConverter = domainEventConverter;
        }

        public void SubscribeToStreamsAndStartLoading()
        {
            foreach (var queryHandler in _handlerList)
            {
                foreach (var subscribedType in queryHandler.SubscribedTypes)
                {
                    var eventStoreStreamCatchUpSubscription = _connection.SubscribeToStreamFrom($"{_eventStoreConfig.EventStream}-{subscribedType.Name}",
                        queryHandler.LastSubscriptionVersion,
                        new CatchUpSubscriptionSettings(int.MaxValue, 100, false, true), HandleSubscription);
                    Subs.Add(eventStoreStreamCatchUpSubscription);
                }
            }
        }

        public IList<EventStoreCatchUpSubscription> Subs { get; private set; } = new List<EventStoreCatchUpSubscription>();

        private Task HandleSubscription(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var domainEvent = _domainEventConverter.Deserialize(resolvedEvent);
            var eventType = domainEvent.GetType();
            var subscribedQueryHandlers =
                _handlerList.Where(handler => handler.SubscribedTypes.Any(type => type == eventType));
            foreach (var queryHandler in subscribedQueryHandlers) queryHandler.Handle(domainEvent, resolvedEvent.OriginalEventNumber);
            return Task.CompletedTask;
        }
    }
}