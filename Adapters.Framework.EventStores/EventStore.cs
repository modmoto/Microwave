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
        private readonly IDomainObjectPersister _domainObjectPersister;
        public IEnumerable<DomainEvent> DomainEvents { get; }

        public EventStore(IDomainObjectPersister domainObjectPersister)
        {
            _domainObjectPersister = domainObjectPersister;
            DomainEvents = domainObjectPersister.Load();
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                DomainEvents.Append(domainEvent);
            }

            await _domainObjectPersister.Store(DomainEvents);
        }

        public T LoadAsync<T>(Guid commandEntityId) where T : Entity, new()
        {
            var entity = new T();
            var domainEventsForEntity = DomainEvents.Where(domainEvent => domainEvent.EntityId == commandEntityId);
            foreach (var domainEvent in domainEventsForEntity)
            {
                entity.Apply(domainEvent);
            }

            return entity;
        }
    }
}