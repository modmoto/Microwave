using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEventStoreConnection _connection;
        private readonly DomainEventConverter _domainEventConverter;
        private readonly EventStoreConfig _eventStoreConfig;
        private readonly IEnumerable<IQueryHandler> _handlerList;

        public QueryEventDelegator(IEnumerable<IQueryHandler> handlerList, IEventStoreConnection connection,
            EventStoreConfig eventStoreConfig, DomainEventConverter domainEventConverter)
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
                    _connection.SubscribeToStreamFrom($"{_eventStoreConfig.EventStream}-{subscribedType.Name}",
                        0,
                        new CatchUpSubscriptionSettings(int.MaxValue, 100, true, true), HandleSubscription);
                }
            }
        }

        private Task HandleSubscription(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var domainEvent = _domainEventConverter.Deserialize(resolvedEvent);
            var eventType = domainEvent.GetType();
            var subscribedQueryHandlers =
                _handlerList.Where(handler => handler.SubscribedTypes.Any(type => type == eventType));
            foreach (var queryHandler in subscribedQueryHandlers)
            {
                queryHandler.Handle(domainEvent);
            }
            return Task.CompletedTask;
        }
    }
}