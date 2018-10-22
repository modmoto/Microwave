using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public class EventDelegateHandler<T> : IEventDelegateHandler where T : DomainEvent
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEnumerable<IHandleAsync<T>> _handles;
        private readonly IVersionRepository _versionRepository;

        public EventDelegateHandler(
            IVersionRepository versionRepository,
            IEventRepository eventRepository,
            IEnumerable<IHandleAsync<T>> handles)
        {
            _versionRepository = versionRepository;
            _eventRepository = eventRepository;
            _handles = handles;
        }

        public async Task Update()
        {
            foreach (var handle in _handles)
            {
                var domainEventType = $"{handle.GetType().Name}-{typeof(T).Name}";
                var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
                var latestEvents = await _eventRepository.LoadEventsByTypeAsync<T>(lastVersion);
                foreach (var latestEvent in latestEvents)
                {
                    lastVersion = lastVersion + 1;
                    await handle.HandleAsync(latestEvent);
                    await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, lastVersion));
                }
            }
        }
    }
}