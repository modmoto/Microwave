using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public class ReadModelHandler<T> where T : IdentifiableQuery, new()
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEventFeed _eventFeed;
        private readonly IVersionRepository _versionRepository;

        public ReadModelHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IEventFeed eventFeed)
        {
            _qeryRepository = qeryRepository;
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
        }

        public async Task Update()
        {
            var redaModelVersionCounter = $"ReadModelVersion-{typeof(T)}";
            var lastVersion = await _versionRepository.GetVersionAsync(redaModelVersionCounter);
            var latestEvents = await _eventFeed.GetEvents(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            foreach (var latestEvent in domainEvents)
            {
                var domainEvent = domainEvents.First().DomainEvent;
                var domainEventEntityId = domainEvent.EntityId;
                var result = await _qeryRepository.Load<T>(domainEventEntityId);
                if (result.Is<NotFound<ReadModelWrapper<T>>>())
                {
                    var wrapper = new ReadModelWrapper<T>(new T(), domainEventEntityId, latestEvent.Version);
                    result = Result<ReadModelWrapper<T>>.Ok(wrapper);
                }

                var readModel = result.Value.ReadModel;
                readModel.Handle(latestEvent.DomainEvent);

                var readModelWrapper = new ReadModelWrapper<T>(readModel, domainEventEntityId, latestEvent.Version);
                await _qeryRepository.SaveById(readModelWrapper);
                await _versionRepository.SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created));
            }
        }
    }
}