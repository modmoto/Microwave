using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public class QueryEventHandler<TQuerry, TEvent> : IQueryEventHandler where TQuerry : Query, new() where TEvent : IDomainEvent
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEventFeed _eventFeed;
        private readonly IVersionRepository _versionRepository;

        public QueryEventHandler(
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
            var domainEventType = $"QuerryHandler-{typeof(TQuerry).Name}-{typeof(TEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            var querry = await _qeryRepository.Load<TQuerry>();
            if (querry.Is<NotFound>()) querry = Result<TQuerry>.Ok(new TQuerry());
            var querryValue = querry.Value;
            foreach (var latestEvent in domainEvents)
            {
                querryValue.Handle(latestEvent.DomainEvent);
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }

            await _qeryRepository.Save(querryValue);
        }
    }
}