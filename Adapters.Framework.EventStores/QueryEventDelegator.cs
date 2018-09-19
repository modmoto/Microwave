using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class QueryEventDelegator
    {
        private readonly IEnumerable<IEventHandler> _handlerList;
        private readonly IEventStoreFacade _facade;

        public QueryEventDelegator(IEnumerable<IEventHandler> handlerList, IEventStoreFacade facade)
        {
            _handlerList = handlerList;
            _facade = facade;
        }

        public void SubscribeToStreamsFrom(long version = 0)
        {
            foreach (var queryHandler in _handlerList)
            {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                {
                    _facade.SubscribeFrom(subscribedType, version, HandleSubscription);
                }
            }
        }

        private void HandleSubscription(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var subscribedQueryHandlers =
                _handlerList.Where(handler => handler.SubscribedDomainEventTypes.Any(type => type == eventType));
            foreach (var queryHandler in subscribedQueryHandlers)
            {
                queryHandler.Handle(domainEvent);
            }
        }

        public async Task SubscribeToStreams()
        {
            foreach (var queryHandler in _handlerList)
            {
                foreach (var subscribedType in queryHandler.SubscribedDomainEventTypes)
                {
                    await _facade.Subscribe(subscribedType, HandleSubscription);
                }
            }
        }
    }
}