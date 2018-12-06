using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Domain;

namespace Microwave.Application
{
    public class EventDelegateHandler<T> : IEventDelegateHandler where T : IDomainEvent
    {
        private readonly IEventFeed<T> _eventFeed;
        private readonly IEnumerable<IHandleAsync<T>> _handles;
        private readonly IVersionRepository _versionRepository;

        public EventDelegateHandler(
            IVersionRepository versionRepository,
            IEventFeed<T> eventFeed,
            IEnumerable<IHandleAsync<T>> handles)
        {
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
            _handles = handles;
        }

        public async Task Update()
        {
            foreach (var handle in _handles)
            {
                var domainEventType = $"{handle.GetType().Name}-{typeof(T).Name}";
                var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
                var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
                foreach (var latestEvent in latestEvents)
                {
                    await handle.HandleAsync(latestEvent.DomainEvent);
                    await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.Created));
                }
            }
        }
    }
}