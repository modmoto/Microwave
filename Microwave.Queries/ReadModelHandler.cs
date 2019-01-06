using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public interface IReadModelHandler
    {
        Task Update();
    }

    public class ReadModelHandler<T> : IReadModelHandler where T : ReadModel, new()
    {
        private readonly IReadModelRepository _readModelRepository;
        private readonly IEventFeed<ReadModelHandler<T>> _eventFeed;
        private readonly IVersionRepository _versionRepository;

        public ReadModelHandler(
            IReadModelRepository readModelRepository,
            IVersionRepository versionRepository,
            IEventFeed<ReadModelHandler<T>> eventFeed)
        {
            _readModelRepository = readModelRepository;
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
        }

        public async Task Update()
        {
            var redaModelVersionCounter = $"ReadModelVersion-{typeof(T)}";
            var lastVersion = await _versionRepository.GetVersionAsync(redaModelVersionCounter);
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            foreach (var latestEvent in domainEvents)
            {
                var latestEventDomainEvent = latestEvent.DomainEvent;
                var domainEventEntityId = latestEventDomainEvent.EntityId;
                var latestEventVersion = latestEvent.Version;

                Result<ReadModelWrapper<T>> result;
                if (new T().GetsCreatedOn == latestEventDomainEvent.GetType())
                {
                    var wrapper = new ReadModelWrapper<T>(new T(), domainEventEntityId, latestEventVersion);
                    result = Result<ReadModelWrapper<T>>.Ok(wrapper);
                }
                else
                {
                    result = await _readModelRepository.Load<T>(domainEventEntityId);
                }

                if (result.Is<NotFound>()) continue;

                var modelWrapper = result.Value;
                var readModel = modelWrapper.ReadModel;
                readModel.Handle(latestEventDomainEvent);

                if (latestEventVersion < modelWrapper.Version) latestEventVersion = modelWrapper.Version;

                var readModelWrapper = new ReadModelWrapper<T>(readModel, domainEventEntityId, latestEventVersion);
                await _readModelRepository.Save(readModelWrapper);
                await _versionRepository
                    .SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created));
            }
        }
    }
}