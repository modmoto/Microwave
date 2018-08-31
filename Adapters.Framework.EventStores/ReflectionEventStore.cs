using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using Newtonsoft.Json;

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
                entity = Apply(entity, domainEvent);
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

        private IDictionary<string, object> ToDictionary<T>(T obj)
        {
            var expando = new Dictionary<string, Object>();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                expando.Add(propertyInfo.Name, currentValue);
            }

            return expando;
        }

        private T Apply<T>(T entity, DomainEvent domainEvent) where T : Entity, new()
        {
            var eventType = domainEvent.GetType();
            var entityType = entity.GetType();
            var entityProperties = entityType.GetProperties();
            var eventProperties = eventType.GetProperties().Where(eventProp => eventProp.Name != nameof(DomainEvent.Id) && eventProp.Name != nameof(DomainEvent.EntityId));

            var dynamicEntity = ToDictionary(entity);

            foreach (var eventProperty in eventProperties)
            {
                var customAttributes = eventProperty.GetCustomAttributes(typeof(ActualPropertyName));
                var attributes = customAttributes.ToList();

                string pathName;
                if (!attributes.Any())
                {
                    pathName = eventProperty.Name;
                }
                else
                {
                    pathName = ((ActualPropertyName) attributes.First()).Path;
                    var propertyOnEntity = entityProperties.FirstOrDefault(property => property.Name == pathName);
                    if (propertyOnEntity == null) throw new ApplicationException($"Property {pathName} does not exist on entity, check the ActualPropertyName Attribute on Property {eventProperty.Name} of Event {domainEvent.GetType().Name}");
                }

                var eventValue = eventProperty.GetValue(domainEvent);
                if (dynamicEntity.ContainsKey(pathName)) dynamicEntity[pathName] = eventValue;
                else
                {
                    dynamicEntity.Add(pathName, eventValue);
                }
            }

            var serializeObject = JsonConvert.SerializeObject(dynamicEntity);
            var deserializeObject = JsonConvert.DeserializeObject<T>(serializeObject);
            return deserializeObject;
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents()
        {
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync();
            return _domainEvents;
        }
    }
}