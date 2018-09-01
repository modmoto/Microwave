using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private T Apply<T>(T entity, DomainEvent domainEvent) where T : Entity, new()
        {
            var eventType = domainEvent.GetType();
            var eventProperties = eventType.GetProperties().Where(eventProp => eventProp.Name != nameof(DomainEvent.Id) && eventProp.Name != nameof(DomainEvent.EntityId));

            var eventJson = JObject.Parse(JsonConvert.SerializeObject(domainEvent));
            var entityJson = JObject.Parse(JsonConvert.SerializeObject(entity));

            foreach (var eventProperty in eventProperties)
            {
                var customAttributes = eventProperty.GetCustomAttributes(typeof(ActualPropertyName));
                var attributes = customAttributes.ToList();

                if (attributes.Any())
                {
                    var pathSplitted = ((ActualPropertyName) attributes.First()).Path;
                    var eventValue = eventProperty.GetValue(domainEvent);
                    var dynamicObjectRenamed = CreateDynamicObject(pathSplitted, new Dictionary<string, object>(), eventValue);
                    MergeOnto(eventJson, dynamicObjectRenamed);

                    var eventPropertyName = eventProperty.Name;
                    var entityValueProperty = entity.GetType().GetProperty(eventPropertyName);
                    if (entityValueProperty != null)
                    {
                        var entityValue = entityValueProperty.GetValue(entity);
                        var dynamicObjectOld = CreateDynamicObject(new [] { eventPropertyName }, new Dictionary<string, object>(), entityValue);
                        MergeOnto(eventJson, dynamicObjectOld);
                    }
                }
            }

            MergeOnto(entityJson, eventJson);
            var deserializeObject = entityJson.ToObject<T>();
            return deserializeObject;
        }

        private void MergeOnto(JObject target, object source)
        {
            var dynamicObjectOldJson = JObject.Parse(JsonConvert.SerializeObject(source));
            target.Merge(dynamicObjectOldJson, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge
            });
        }

        private IDictionary<string, object> CreateDynamicObject(string[] pathSplitted, Dictionary<string, object> dictionary, object eventValue)
        {
            if (pathSplitted.Length == 1)
            {
                var dic = new Dictionary<string, object>();
                dic.Add(pathSplitted[0], eventValue);
                return dic;
            }

            var dynamicObject = CreateDynamicObject(pathSplitted.Skip(1).ToArray(), dictionary, eventValue);
            dictionary.Add(pathSplitted[0], dynamicObject);
            return dictionary;
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents()
        {
            if (_domainEvents == null) _domainEvents = await _domainEventPersister.GetAsync();
            return _domainEvents;
        }


    }
}