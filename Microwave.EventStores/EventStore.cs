using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;
using Microwave.EventStores.Ports;
using Microwave.EventStores.SnapShots;

namespace Microwave.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEventRepository _eventRepository;
        private readonly ISnapShotRepository _snapShotRepository;
        private readonly ISnapShotConfig _snapShotConfig;

        public EventStore(
            IEventRepository eventRepository,
            ISnapShotRepository snapShotRepository,
            ISnapShotConfig snapShotConfig = null)
        {
            _eventRepository = eventRepository;
            _snapShotRepository = snapShotRepository;
            _snapShotConfig = snapShotConfig ?? new SnapShotConfig();
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var differentIds = events.GroupBy(de => de.EntityId).ToList();
            if (differentIds.Count > 1) throw new DifferentIdsException(differentIds.Select(g => g.Key));
            var result = await _eventRepository.AppendAsync(events, entityVersion);
            return result;
        }

        public Task<Result> AppendAsync(IDomainEvent domainEvent, long entityVersion)
        {
            return AppendAsync(new[] {domainEvent}, entityVersion);
        }

        public async Task<EventStoreResult<T>> LoadAsync<T>(string entityId) where T : IApply, new()
        {
            if (entityId == null) return EventStoreResult<T>.NotFound(null);
            var snapShot = await _snapShotRepository.LoadSnapShot<T>(entityId);
            var entity = snapShot.Value;
            var result = await _eventRepository.LoadEventsByEntity(entityId, snapShot.Version);
            if (result.Is<NotFound>()) return EventStoreResult<T>.NotFound(entityId);
            var domainEventWrappers = result.Value.ToList();
            entity.Apply(domainEventWrappers.Select(ev => ev.DomainEvent));
            var version = domainEventWrappers.LastOrDefault()?.Version ?? snapShot.Version;
            if (_snapShotConfig.NeedSnapshot<T>(snapShot.Version, version))
                await _snapShotRepository.SaveSnapShot(new SnapShotWrapper<T>(entity, entityId, version));
            return EventStoreResult<T>.Ok(entity, version);
        }
    }
}