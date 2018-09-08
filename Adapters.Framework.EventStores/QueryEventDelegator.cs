using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        public readonly IEnumerable<IQueryHandler> HandlerList;
        private readonly IEventStoreConnection _connection;
        private readonly IDomainEventConverter _domainEventConverter;

        public QueryEventDelegator(IEnumerable<IQueryHandler> handlerList, IEventStoreConnection connection, EventStoreConfig eventStoreConfig, IDomainEventConverter domainEventConverter)
        {
            var queryHandlers = handlerList.ToList();
            HandlerList = queryHandlers;
            _connection = connection;
            _connection.ConnectAsync().Wait();
            _domainEventConverter = domainEventConverter;
            var subscriptionTypes = HandlerList.SelectMany(handler => handler.SubscribedTypes);
            foreach (var queryHandler in subscriptionTypes)
            {
                var eventStoreSubscription = connection.SubscribeToStreamAsync($"{eventStoreConfig.EventStream}-{queryHandler.Name}", true, HandleSubscription).Result;
            }
        }

        private Task HandleSubscription(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var domainEvent = _domainEventConverter.Deserialize(resolvedEvent);
            var eventType = domainEvent.GetType();
            var subscribedQueryHandlers = HandlerList.Where(handler => handler.SubscribedTypes.Any(type => type == eventType));
            foreach (var queryHandler in subscribedQueryHandlers)
            {
                queryHandler.Handle(domainEvent);
            }
            return Task.CompletedTask;
        }
    }
}