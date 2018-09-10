using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEnumerable<IQueryHandler> _handlerList;
        private readonly IEventStoreConnection _connection;
        private readonly EventStoreConfig _eventStoreConfig;
        private readonly IDomainEventConverter _domainEventConverter;

        public QueryEventDelegator(IEnumerable<IQueryHandler> handlerList, IEventStoreConnection connection, EventStoreConfig eventStoreConfig, IDomainEventConverter domainEventConverter)
        {
            _handlerList = handlerList;
            _connection = connection;
            _eventStoreConfig = eventStoreConfig;
            _domainEventConverter = domainEventConverter;
        }

        public async Task SubscribeToStreams()
        {
            var subscriptionTypes = _handlerList.SelectMany(handler => handler.SubscribedTypes);
            foreach (var queryHandler in subscriptionTypes)
            {
                await _connection.SubscribeToStreamAsync($"{_eventStoreConfig.EventStream}-{queryHandler.Name}", true, HandleSubscription);
            }
        }

        private Task HandleSubscription(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var domainEvent = _domainEventConverter.Deserialize(resolvedEvent);
            var eventType = domainEvent.GetType();
            var subscribedQueryHandlers = _handlerList.Where(handler => handler.SubscribedTypes.Any(type => type == eventType));
            foreach (var queryHandler in subscribedQueryHandlers)
            {
                queryHandler.Handle(domainEvent);
            }
            return Task.CompletedTask;
        }
    }
}