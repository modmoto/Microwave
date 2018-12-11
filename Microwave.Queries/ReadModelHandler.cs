using System;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IReadModelHandler
    {
        Task Update();
    }

    public class ReadModelHandler<T> : IReadModelHandler where T : ReadModel, new()
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEventFeed<ReadModelHandler<T>> _eventFeed;
        private readonly IEventLocationConfig _eventLocationConfig;
        private readonly IVersionRepository _versionRepository;

        public ReadModelHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IEventFeed<ReadModelHandler<T>> eventFeed,
            IEventLocationConfig eventLocationConfig)
        {
            _qeryRepository = qeryRepository;
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
            _eventLocationConfig = eventLocationConfig;
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
                if (IsCreationEvent(latestEventDomainEvent))
                {
                    var wrapper = new ReadModelWrapper<T>(new T(), domainEventEntityId, latestEventVersion);
                    result = Result<ReadModelWrapper<T>>.Ok(wrapper);
                }
                else
                {
                    try
                    {
                        result = _qeryRepository.Load<T>(domainEventEntityId).Result;
                    }
                    catch (AggregateException e)
                    {
                        continue;
                    }
                }

                if (result.Is<NotFound>()) continue;

                var modelWrapper = result.Value;
                var readModel = modelWrapper.ReadModel;
                readModel.Handle(latestEventDomainEvent);

                if (latestEventVersion < modelWrapper.Version) latestEventVersion = modelWrapper.Version;

                var readModelWrapper = new ReadModelWrapper<T>(readModel, domainEventEntityId, latestEventVersion);
                _qeryRepository.Save(readModelWrapper).Wait();
                _versionRepository
                    .SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created)).Wait();
            }
        }

        private bool IsCreationEvent(IDomainEvent latestEventDomainEvent)
        {
            var eventName = latestEventDomainEvent.GetType().Name;
            var readModelName = typeof(T).Name;
            return _eventLocationConfig.GetCreationEventForReadModel(readModelName) == eventName;
        }
    }
}