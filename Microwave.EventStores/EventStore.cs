using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application.Ports;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEntityStreamRepository _entityStreamRepository;

        public EventStore(IEntityStreamRepository entityStreamRepository)
        {
            _entityStreamRepository = entityStreamRepository;
        }

        public async Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var result = await _entityStreamRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : IApply, new()
        {
            var entity = new T();
            var domainEvents = (await _entityStreamRepository.LoadEventsByEntity(entityId)).Value;
            var domainEventWrappers = domainEvents.ToList();
            entity.Apply(domainEventWrappers.Select(ev => ev.DomainEvent));
            return new EventstoreResult<T>(domainEventWrappers.Last().Version, entity);
        }
    }
}