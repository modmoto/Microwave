using System.Linq;
using System.Threading.Tasks;
using Application.Framework.Results;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQueryEventHandler
    {
        Task Update();
    }

    public class QueryEventHandler<TQuerry, TEvent> : IQueryEventHandler where TQuerry : Query, new() where TEvent : DomainEvent
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IPublishedEventStream<TEvent> _eventRepository;
        private readonly IVersionRepository _versionRepository;

        public QueryEventHandler(
            IQeryRepository qeryRepository,
            IVersionRepository versionRepository,
            IPublishedEventStream<TEvent> eventRepository)
        {
            _qeryRepository = qeryRepository;
            _versionRepository = versionRepository;
            _eventRepository = eventRepository;
        }

        public async Task Update()
        {
            var domainEventType = $"QuerryHandler-{typeof(TQuerry).Name}-{typeof(TEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var latestEvents = await _eventRepository.GetEventsByTypeAsync(lastVersion);
            var domainEvents = latestEvents.ToList();
            if (!domainEvents.Any()) return;

            var querry = await _qeryRepository.Load<TQuerry>();
            if (querry.Is<NotFound<TQuerry>>()) querry = Result<TQuerry>.Ok(new TQuerry());
            var querryValue = querry.Value;
            foreach (var latestEvent in domainEvents)
            {
                lastVersion = lastVersion + 1;
                querryValue.Handle(latestEvent);
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, lastVersion));
            }

            await _qeryRepository.Save(querryValue);
        }
    }

    public class IdentifiableQueryEventHandler<T, TEvent> where T : IdentifiableQuery where TEvent : DomainEvent
    {
        private readonly IQeryRepository _qeryRepository;

        public IdentifiableQueryEventHandler(IQeryRepository qeryRepository)
        {
            _qeryRepository = qeryRepository;
        }

        public async Task HandleAsync(TEvent domainEvent)
        {
            var result = await _qeryRepository.Load<T>(domainEvent.EntityId);
            var identifiableQuery = result.Value;
            identifiableQuery.Handle(domainEvent);
            await _qeryRepository.Save(identifiableQuery);
        }
    }
}