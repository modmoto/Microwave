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
        private readonly IEventRepository _eventRepository;

        public EventStore(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var result = await _eventRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : IApply, new()
        {
            var entity = new T();
            var domainEvents = (await _eventRepository.LoadEventsByEntity(entityId)).Value;
            var domainEventWrappers = domainEvents.ToList();
            entity.Apply(domainEventWrappers.Select(ev => ev.DomainEvent));
            return new EventstoreResult<T>(domainEventWrappers.Last().Version, entity);
        }
    }
}