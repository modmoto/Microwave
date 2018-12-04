using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public class ReadModelHandler<T> where T : ReadModel, new()
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
                var latestEventDomainEvent = latestEvent.DomainEvent;
                var result = await _qeryRepository.Load<T>(latestEventDomainEvent.EntityId);

                if (result.Is<NotFound<T>>()) result = Result<T>.Ok(new T());

                var readModel = result.Value;
                readModel.Handle(latestEventDomainEvent);
                readModel.Version = latestEvent.Version;

                await _qeryRepository.Save(readModel);
                await _versionRepository.SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created));
            }
        }
    }
}