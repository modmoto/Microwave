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
        private readonly IEventFeed<ReadModelHandler<T>>  _eventFeed;
        private readonly IVersionRepository _versionRepository;
        private readonly object _lock = new object();

        public ReadModelHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IEventFeed<ReadModelHandler<T>> eventFeed)
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
                    var latestEventDomainEvent = latestEvent.DomainEvent;
                    if (ReadModelDoesNotHandleEvent(latestEventDomainEvent)) continue;
                    var domainEventEntityId = latestEventDomainEvent.EntityId;
                    var result = _qeryRepository.Load<T>(domainEventEntityId).Result;
                    var latestEventVersion = latestEvent.Version;
                    if (result.Is<NotFound>())
                    {
                        var wrapper = new ReadModelWrapper<T>(new T(), domainEventEntityId, latestEventVersion);
                        result = Result<ReadModelWrapper<T>>.Ok(wrapper);
                    }

                    var modelWrapper = result.Value;
                    var readModel = modelWrapper.ReadModel;
                    readModel.Handle(latestEventDomainEvent);

                    if (latestEventVersion < modelWrapper.Version) latestEventVersion = modelWrapper.Version;

                    var readModelWrapper = new ReadModelWrapper<T>(readModel, domainEventEntityId, latestEventVersion);
                    _qeryRepository.Save(readModelWrapper).Wait();
                    _versionRepository.SaveVersion(new LastProcessedVersion(redaModelVersionCounter, latestEvent.Created)).Wait();
                }
            }
        }

        private bool ReadModelDoesNotHandleEvent(IDomainEvent latestEventDomainEvent)
        {
            var readModelType = typeof(T);
            var methods = readModelType.GetMethods();
            var methodInfos = methods.Where(m =>
                m.Name == nameof(Query.Handle) && m.GetParameters().Length == 1&& m.GetParameters().First().ParameterType == latestEventDomainEvent.GetType());
            return !methodInfos.Any();
        }
    }
}