using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores.Ports;

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

        public async Task<Result<EventStoreResult<T>>> LoadAsync<T>(string entityId) where T : IApply, new()
        {
            var snapShot = await _snapShotRepository.LoadSnapShot<T>(entityId);
            var entity = snapShot.Entity;
            var result = await _eventRepository.LoadEventsByEntity(entityId, snapShot.Version);
            if (result.Is<NotFound>()) return Result<EventStoreResult<T>>.NotFound(entityId);
            var domainEventWrappers = result.Value.ToList();
            entity.Apply(domainEventWrappers.Select(ev => ev.DomainEvent));
            var version = domainEventWrappers.LastOrDefault()?.Version ?? snapShot.Version;
            if (NeedSnapshot(typeof(T), snapShot.Version, version))
                await _snapShotRepository.SaveSnapShot(new SnapShotWrapper<T>(entity, entityId, version));
            return Result<EventStoreResult<T>>.Ok(new EventStoreResult<T>(entity, version));
        }

        private bool NeedSnapshot(Type type, long snapShotVersion, long version)
        {
            if (!(type.GetCustomAttribute(typeof(SnapShotAfterAttribute)) is SnapShotAfterAttribute customAttribute)) return false;
            return customAttribute.DoesNeedSnapshot(snapShotVersion, version);
        }
    }
}