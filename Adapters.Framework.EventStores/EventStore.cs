using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IDomainEventPersister _domainEventPersister;
        public IEnumerable<DomainEvent> DomainEvents { get; private set; }

        public EventStore(IDomainEventPersister domainEventPersister)
        {
            _domainEventPersister = domainEventPersister;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents)
        {
            if (DomainEvents == null) DomainEvents = await _domainEventPersister.GetAsync();
            var events = DomainEvents.ToList();
            foreach (var domainEvent in domainEvents)
            {
                events.Append(domainEvent);
            }

            await _domainEventPersister.Save(events);
        }

        public async Task AppendAsync(DomainEvent domainEvent)
        {
            if (DomainEvents == null) DomainEvents = await _domainEventPersister.GetAsync();
            var events = DomainEvents.ToList();
            events.Append(domainEvent);
            await _domainEventPersister.Save(events);
        }

        public async Task<T> LoadAsync<T>(Guid commandEntityId) where T : Entity, new()
        {
            var entity = new T();
            if (DomainEvents == null) DomainEvents = await _domainEventPersister.GetAsync();
            var domainEventsForEntity = DomainEvents.Where(domainEvent => domainEvent.EntityId == commandEntityId);
            foreach (var domainEvent in domainEventsForEntity)
            {
                entity.Apply(domainEvent);
            }

            return entity;
        }
    }
}