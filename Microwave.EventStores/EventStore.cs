using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEventRepository _eventRepository;
        private readonly ISnapShotRepository _snapShotRepository;

        public EventStore(IEventRepository eventRepository, ISnapShotRepository snapShotRepository)
        {
            _eventRepository = eventRepository;
            _snapShotRepository = snapShotRepository;
        }

        public async Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var result = await _eventRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : IApply, new()
        {
            var snapShot = await _snapShotRepository.LoadSnapShot<T>(entityId);
            var entity = snapShot.Entity;
            var result = await _eventRepository.LoadEventsByEntity(entityId, snapShot.Version);
            var domainEventWrappers = result.Value.ToList();
            entity.Apply(domainEventWrappers.Select(ev => ev.DomainEvent));
            var version = domainEventWrappers.Last().Version;
            if (NeedSnapshot(typeof(T), snapShot.Version, version))
                await _snapShotRepository.SaveSnapShot(entity, entityId, version);
            return new EventstoreResult<T>(version, entity);
        }

        private bool NeedSnapshot(Type type, long snapShotVersion, long version)
        {
            var customAttribute = type.GetCustomAttribute(typeof(SnapShotAfterAttribute)) as SnapShotAfterAttribute;
            if (customAttribute == null) return false;
            return customAttribute.DoesNeedSnapshot(snapShotVersion, version);
        }
    }
}