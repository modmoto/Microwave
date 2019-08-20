using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.Results;
using Microwave.Queries.Ports;

namespace Microwave.Queries.Handler
{
    public class QueryEventHandler<TQuerry, TEvent> : IQueryEventHandler where TQuerry : Query, new()
    {
        private readonly IReadModelRepository _readModelRepository;
        private readonly IEventFeed<QueryEventHandler<TQuerry, TEvent>> _eventFeed;
        private readonly IVersionRepository _versionRepository;

        public QueryEventHandler(
            IReadModelRepository readModelRepository,
            IVersionRepository versionRepository,
            IEventFeed<QueryEventHandler<TQuerry, TEvent>> eventFeed)
        {
            _readModelRepository = readModelRepository;
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
        }

        public async Task Update()
        {
            var domainEventType = $"QuerryHandler-{typeof(TQuerry).Name}-{typeof(TEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var lastVersionRemote = await _versionRepository.GetRemoteVersionAsync(domainEventType);
            if (lastVersion > lastVersionRemote) return;
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            var domainEvents = latestEvents.ToList();

            var querry = await _readModelRepository.LoadAsync<TQuerry>();
            if (querry.Is<NotFound>()) querry = Result<TQuerry>.Ok(new TQuerry());
            var querryValue = querry.Value;
            foreach (var latestEvent in domainEvents)
            {
                querryValue.Handle(latestEvent.DomainEvent, latestEvent.Version);
                await _versionRepository.SaveVersionAsync(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }

            await _readModelRepository.SaveQueryAsync(querryValue);
        }
    }
}