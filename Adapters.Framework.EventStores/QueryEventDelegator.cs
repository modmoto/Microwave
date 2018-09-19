using System.Collections.Generic;
using System.Linq;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEventStoreFacade _facade;
        private readonly IEnumerable<IEventHandler> _handlerList;

        public QueryEventDelegator(IEnumerable<IEventHandler> handlerList, IEventStoreFacade facade)
        {
            _handlerList = handlerList;
            _facade = facade;
        }

        public void SubscribeToStreams()
        {
            var queryHandlers = _handlerList.Where(handler =>
                handler.GetType().BaseType.IsGenericType && handler.GetType().BaseType.GetGenericTypeDefinition() ==
                typeof(QueryEventHandler<>));
            foreach (var queryHandler in queryHandlers) {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                    _facade.SubscribeFrom(subscribedType, 0, HandleSubscription);
            }

            var eventHandlers = _handlerList.Where(handler =>
                handler.GetType().BaseType.IsGenericType && handler.GetType().BaseType.GetGenericTypeDefinition() ==
                typeof(ReactiveEventHandler<>));
            foreach (var queryHandler in eventHandlers)
            {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                    _facade.Subscribe(subscribedType, HandleSubscription);
            }
        }

        private void HandleSubscription(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlers = _handlerList.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var handler in handlers) handler.Handle(domainEvent);
        }
    }
}