using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEventStoreFacade _facade;
        private readonly IEnumerable<IEventHandler> _eventHandlers;
        private readonly IEnumerable<IEventHandler> _queryHandlers;

        public QueryEventDelegator(IEnumerable<IEventHandler> handlerList, IEventStoreFacade facade)
        {
            var enumerable = handlerList.ToList();
            _queryHandlers = enumerable.Where(handler =>
                handler.GetType().BaseType.IsGenericType && handler.GetType().BaseType.GetGenericTypeDefinition() ==
                typeof(QueryEventHandler<>));
            _eventHandlers = enumerable.Where(handler =>
                handler.GetType().BaseType.IsGenericType && handler.GetType().BaseType.GetGenericTypeDefinition() ==
                typeof(ReactiveEventHandler<>));

            _facade = facade;
        }

        public void SubscribeToStreams()
        {
            foreach (var queryHandler in _queryHandlers) {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                    _facade.SubscribeFrom(subscribedType, 0, HandleQuerySubscription);
            }

            foreach (var queryHandler in _eventHandlers)
            {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                    _facade.Subscribe(subscribedType, HandleEventSubscription);
            }
        }

        private void HandleEventSubscription(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlers = _eventHandlers.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var handler in handlers) handler.Handle(domainEvent);
        }

        private void HandleQuerySubscription(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlers = _queryHandlers.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var handler in handlers) handler.Handle(domainEvent);
        }
    }
}