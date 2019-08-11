using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.Results;
using Microwave.Queries.Ports;

namespace Microwave.Queries.Handler
{
    public interface IReadModelEventHandler
    {
        Task Update();
    }

    public class ReadModelEventHandler<T> : IReadModelEventHandler where T : ReadModel, new()
    {
        private readonly IReadModelRepository _readModelRepository;
        private readonly IEventFeed<ReadModelEventHandler<T>> _eventFeed;
        private readonly IVersionRepository _versionRepository;

        public ReadModelEventHandler(
            IReadModelRepository readModelRepository,
            IVersionRepository versionRepository,
            IEventFeed<ReadModelEventHandler<T>> eventFeed)
        {
            _readModelRepository = readModelRepository;
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
        }

        public async Task Update()
        {
            var redaModelVersionCounter = $"ReadModelVersion-{typeof(T).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(redaModelVersionCounter);
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            foreach (var latestEvent in domainEvents)
            {
                var latestEventDomainEvent = latestEvent.DomainEvent;
                var domainEventEntityId = latestEventDomainEvent.EntityId;
                var latestEventVersion = latestEvent.Version;

                var result = new T().GetsCreatedOn == latestEventDomainEvent.GetType()
                    ? Result<T>.Ok(new T())
                    : await _readModelRepository.LoadAsync<T>(domainEventEntityId);

                if (result.Is<NotFound>()) continue;

                var readModel = result.Value;
                readModel.Handle(latestEventDomainEvent, latestEventVersion);

                if (latestEventVersion < result.Value.Version) latestEventVersion = result.Value.Version;

                readModel.Version = latestEventVersion;
                readModel.Identity = domainEventEntityId;

                await _readModelRepository.SaveReadModelAsync(readModel);
                await _versionRepository.SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created));
            }
        }
    }
}