using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEnumerable<IQueryHandler> _handlerList;
        private readonly IDomainEventConverter _domainEventConverter;

        public QueryEventDelegator(IEnumerable<IQueryHandler> handlerList, IEventStoreConnection connection, EventStoreConfig eventStoreConfig, IDomainEventConverter domainEventConverter)
        {
            var queryHandlers = handlerList.ToList();
            _handlerList = queryHandlers;
            _domainEventConverter = domainEventConverter;
            var subscriptionTypes = _handlerList.SelectMany(handler => handler.SubscribedTypes);
            foreach (var queryHandler in subscriptionTypes)
            {
                connection.SubscribeToStreamAsync($"{eventStoreConfig.EventStream}-{queryHandler.Name}", true, HandleSubscription);
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