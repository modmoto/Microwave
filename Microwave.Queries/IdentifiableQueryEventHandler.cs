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
                var domainEvent = domainEvents.First().DomainEvent;
                var domainEventEntityId = domainEvent.EntityId;
                var result = await _qeryRepository.Load<TQuerry>(domainEventEntityId);
                if (result.Is<NotFound<ReadModelWrapper<TQuerry>>>())
                {
                    var wrapper = new ReadModelWrapper<TQuerry>(new TQuerry(), domainEventEntityId, latestEvent.Version);
                    result = Result<ReadModelWrapper<TQuerry>>.Ok(wrapper);
                }

                var readModel = result.Value.ReadModel;
                readModel.Handle(latestEvent.DomainEvent);

                var readModelWrapper = new ReadModelWrapper<TQuerry>(readModel, domainEventEntityId, latestEvent.Version);
                await _qeryRepository.SaveById(readModelWrapper);
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }
        }
    }
}