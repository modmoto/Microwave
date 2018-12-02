using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Ports;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEntityStreamRepository _entityStreamRepository;
        private readonly IEventSourcingStrategy _eventSourcingStrategy;

        public EventStore(IEntityStreamRepository entityStreamRepository, IEventSourcingStrategy eventSourcingStrategy)
        {
            _entityStreamRepository = entityStreamRepository;
            _eventSourcingStrategy = eventSourcingStrategy;
        }

        public async Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var result = await _entityStreamRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : new()
        {
            var entity = new T();
            var domainEvents = (await _entityStreamRepository.LoadEventsByEntity(entityId)).Value;
            var eventList = domainEvents.ToList();
            entity = eventList.Aggregate(entity, (current, domainEvent) => _eventSourcingStrategy.Apply(current, domainEvent.DomainEvent));
            // TODO get this from stream/events
            return new EventstoreResult<T>(eventList.Count, entity);
        }
    }
}