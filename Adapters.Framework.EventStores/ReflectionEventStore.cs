using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class ReflectionEventStore : IEventStore
    {
        private readonly IDomainEventPersister _domainEventPersister;
        private IEnumerable<DomainEvent> _domainEvents;

        public ReflectionEventStore(IDomainEventPersister domainEventPersister)
        {
            _domainEventPersister = domainEventPersister;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents)
        {
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync() ?? new List<DomainEvent>();

            var domainEventsTemp = _domainEvents.ToList();
            foreach (var domainEvent in domainEvents)
            {
                domainEventsTemp.Add(domainEvent);
            }

            _domainEvents = domainEventsTemp;
            await _domainEventPersister.Save(domainEventsTemp);
        }

        public async Task AppendAsync(DomainEvent domainEvent)
        {
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync();
            var domainEventsTemp = _domainEvents.ToList();
            domainEventsTemp.Add(domainEvent);
            _domainEvents = domainEventsTemp;
            await _domainEventPersister.Save(domainEventsTemp);
        }

        public async Task<T> LoadAsync<T>(Guid commandEntityId) where T : Entity, new()
        {
            var entity = new T();
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync();
            var domainEventsForEntity = _domainEvents.Where(domainEvent => domainEvent.EntityId == commandEntityId);
            foreach (var domainEvent in domainEventsForEntity)
            {
                Apply(entity, domainEvent);
            }

            SetId(entity, commandEntityId);

            return entity;
        }

        private void SetId<T>(T entity, Guid commandEntityId) where T : Entity, new()
        {
            var entityType = entity.GetType();
            var entityId = entityType.GetProperties().First(prop => prop.Name == nameof(Entity.Id));
            entityId.SetValue(entity, commandEntityId);
        }

        private void Apply<T>(T entity, DomainEvent domainEvent) where T : Entity, new()
        {
            var eventType = domainEvent.GetType();
            var entityType = entity.GetType();
            var entityProperties = entityType.GetProperties();
            var eventProperties = eventType.GetProperties().Where(eventProp => eventProp.Name != nameof(DomainEvent.Id) && eventProp.Name != nameof(DomainEvent.EntityId));

            foreach (var eventProperty in eventProperties)
            {
                var customAttributes = eventProperty.GetCustomAttributes(typeof(PropertyPath));
                var attributes = customAttributes.ToList();
                var pathName = !attributes.Any() ? eventProperty.Name : ((PropertyPath) attributes.First()).PropetyName;

                var propertyOnEntity = entityProperties.First(property => property.Name == pathName);
                var eventValue = eventProperty.GetValue(domainEvent);
                propertyOnEntity.SetValue(entity, eventValue);
            }
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents()
        {
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync();
            return _domainEvents;
        }
    }
}