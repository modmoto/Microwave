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

    public class ReadModelHandler<T, TEvent> : IReadModelHandler where T : ReadModel, new() where TEvent : IDomainEvent
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEventFeed<TEvent> _eventFeed;
        private readonly IVersionRepository _versionRepository;
        private object _lock = new object();

        public ReadModelHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IEventFeed<TEvent> eventFeed)
        {
            _qeryRepository = qeryRepository;
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
                lock (_lock)
                {
                    var domainEvent = domainEvents.First().DomainEvent;
                    var domainEventEntityId = domainEvent.EntityId;
                    var result = _qeryRepository.Load<T>(domainEventEntityId).Result;
                    var latestEventVersion = latestEvent.Version;
                    if (result.Is<NotFound<ReadModelWrapper<T>>>())
                    {
                        var wrapper = new ReadModelWrapper<T>(new T(), domainEventEntityId, latestEventVersion);
                        result = Result<ReadModelWrapper<T>>.Ok(wrapper);
                    }

                    var modelWrapper = result.Value;
                    var readModel = modelWrapper.ReadModel;
                    readModel.Handle(latestEvent.DomainEvent);

                    if (latestEventVersion < modelWrapper.Version) latestEventVersion = modelWrapper.Version;

                    var readModelWrapper = new ReadModelWrapper<T>(readModel, domainEventEntityId, latestEventVersion);
                    _qeryRepository.Save(readModelWrapper).Wait();
                    _versionRepository.SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created)).Wait();
                }
            }
        }
    }
}