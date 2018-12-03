using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IIdentifiableQueryEventHandler
    {
        Task Update();
    }

    public class IdentifiableQueryEventHandler<TQuerry, TEvent> :
        IIdentifiableQueryEventHandler
        where TQuerry : IdentifiableQuery, new()
        where TEvent : IDomainEvent
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEventFeed<TEvent> _eventRepository;
        private readonly IVersionRepository _versionRepository;

        public IdentifiableQueryEventHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IEventFeed<TEvent> eventRepository)
        {
            _qeryRepository = qeryRepository;
            _versionRepository = versionRepository;
            _eventRepository = eventRepository;
        }

        public async Task Update()
        {
            var domainEventType = $"IdentifiableQuerryHandler-{typeof(TQuerry).Name}-{typeof(TEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var latestEvents = await _eventRepository.GetEventsByTypeAsync(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            foreach (var latestEvent in domainEvents)
            {
                var result = await _qeryRepository.Load<TQuerry>(domainEvents.First().DomainEvent.EntityId);
                if (result.Is<NotFound<TQuerry>>()) result = Result<TQuerry>.Ok(new TQuerry());

                result.Value.Handle(latestEvent.DomainEvent);

                await _qeryRepository.Save(result.Value);
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }
        }
    }
}