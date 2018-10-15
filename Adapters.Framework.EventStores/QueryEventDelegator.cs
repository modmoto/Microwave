using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEnumerable<IReactiveEventHandler> _eventHandlers;
        private readonly IEventStoreFacade _facade;
        private readonly IEnumerable<IQuerryEventHandler> _queryHandlers;
        private readonly IHandlerVersionRepository _versionRepository;
        private readonly IEventStoreSubscribtion _storeSubscribtion;

        public QueryEventDelegator(IEnumerable<IQuerryEventHandler> handlerList, IEventStoreFacade facade,
            IHandlerVersionRepository versionRepository, IEventStoreSubscribtion storeSubscribtion, IEnumerable<IReactiveEventHandler> reactiveEventHandlers)
        {
            var enumerable = handlerList.ToList();
            _queryHandlers = enumerable.Where(handler =>
                handler.GetType().BaseType.IsGenericType && handler.GetType().BaseType.GetGenericTypeDefinition() ==
                typeof(QueryEventHandler<>));
            _eventHandlers = reactiveEventHandlers;

            _facade = facade;
            _versionRepository = versionRepository;
            _storeSubscribtion = storeSubscribtion;
        }

        public void SubscribeToStreams()
        {
            foreach (var queryHandler in _queryHandlers)
            foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                _storeSubscribtion.SubscribeFrom(subscribedType, 0, HandleQuerySubscription);

            foreach (var eventHandler in _eventHandlers)
            foreach (var subscribedType in eventHandler.SubscribedDomainEventTypes)
            {
                var lastProcessedEvent =
                    _versionRepository.GetLastProcessedVersion(eventHandler, subscribedType).Result;
                _storeSubscribtion.SubscribeFrom(subscribedType, lastProcessedEvent, HandleEventSubscription);
            }
        }

        private async Task HandleEventSubscription(DomainEvent domainEvent, StreamVersion version)
        {
            var eventType = domainEvent.GetType().Name;
            var handlers =
                _eventHandlers.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var handler in handlers) await handler.Handle(domainEvent, version);
        }

        private Task HandleQuerySubscription(DomainEvent domainEvent, StreamVersion version)
        {
            var eventType = domainEvent.GetType().Name;
            var handlers =
                _queryHandlers.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var handler in handlers) return handler.Handle(domainEvent);
            return Task.CompletedTask;
        }
    }
}