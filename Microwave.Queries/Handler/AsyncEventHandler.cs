using System;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Queries.Handler
{
    public class AsyncEventHandler<TConcreteHandlerType, TSubscribedDomainEvent>
        : IEventHandler where TSubscribedDomainEvent : ISubscribedDomainEvent
    {
        private readonly IEventFeed<AsyncEventHandler<TConcreteHandlerType, TSubscribedDomainEvent>>  _eventFeed;
        private readonly IHandleAsync<TSubscribedDomainEvent> _handler;
        private readonly IVersionRepository _versionRepository;
        public Type HandlerClassType => _handler.GetType();

        public AsyncEventHandler(
            IVersionRepository versionRepository,
            IEventFeed<AsyncEventHandler<TConcreteHandlerType, TSubscribedDomainEvent>> eventFeed,
            IHandleAsync<TSubscribedDomainEvent> handler)
        {
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
            _handler = handler;
        }

        public async Task Update()
        {
            var handleType = _handler.GetType();
            var domainEventType = $"{handleType.Name}-{typeof(TSubscribedDomainEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            foreach (var latestEvent in latestEvents)
            {
                if (latestEvent.DomainEvent is TSubscribedDomainEvent domainEvent)
                {
                    await _handler.HandleAsync(domainEvent);
                    await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.OverallVersion));
                }
            }
        }
    }
}