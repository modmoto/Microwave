using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adapters.Framework.EventStores
{
    public class EventStoreFacade : IEventStoreFacade
    {
        private readonly DomainEventConverter _eventConverter;
        private readonly IEventSourcingStrategy _eventSourcingStrategy;
        private readonly EventStoreConfig _eventStoreConfig;
        private readonly IEventStoreConnection _eventStoreConnection;

        public EventStoreFacade(IEventSourcingStrategy eventSourcingStrategy,
            IEventStoreConnection eventStoreConnection, EventStoreConfig eventStoreConfig,
            DomainEventConverter eventConverter)
        {
            _eventSourcingStrategy = eventSourcingStrategy;
            _eventStoreConnection = eventStoreConnection;
            _eventStoreConfig = eventStoreConfig;
            _eventConverter = eventConverter;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var convertedElements = domainEvents.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(_eventConverter.Serialize(eve)), null));
            await _eventStoreConnection.AppendToStreamAsync(_eventStoreConfig.EventStream, entityVersion,
                convertedElements);
        }

        public async Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId, bool loadNestedEntities = false) where T : new()
        {
            var events = await GetEvents(commandEntityId);
            var entity = new T();
            foreach (var domainEvent in events.Result) entity = _eventSourcingStrategy.Apply(entity, domainEvent);

            var type = typeof(T);
            if (AlsoLoad.Count > 0)
            {
                var alsoLoad = AlsoLoad.ToList();
                AlsoLoad = new List<string>();

                foreach (var loadKey in alsoLoad)
                {
                    var propertyInfos = type.GetProperty(loadKey);
                    var entityType = propertyInfos.PropertyType;
                    var methodInfo = GetType().GetMethod("LoadAsync");
                    var makeGenericMethod = methodInfo.MakeGenericMethod(entityType);

                    var guid = ((Entity) propertyInfos.GetValue(entity)).Id;
                    var invoke = (Task) makeGenericMethod.Invoke(this, new object[]{ guid, true });
                    await invoke;
                    var propertyInfo = invoke.GetType().GetProperty("Result");
                    var value = propertyInfo.GetValue(invoke);
                    var ent = value.GetType().GetProperty("Result").GetValue(value);
                    entity = Merge(entity, ent, loadKey);
                }
            }

            return EventStoreResult<T>.Ok(entity, events.EntityVersion);
        }

        public T Merge<T>(T entity, object nestedEntity, string path)
        {
            var pathSplitted = path.Split(".");

            var entityJson = JObject.Parse(JsonConvert.SerializeObject(entity));


            var jObject = JObject.Parse(JsonConvert.SerializeObject(nestedEntity));
            var dynamicObjectOld = CreateDynamicObject(pathSplitted, new Dictionary<string, object>(), jObject);

            var serializeObject = JObject.Parse(JsonConvert.SerializeObject(dynamicObjectOld));

            entityJson.Merge(serializeObject, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge
            });

            var deserializeObject = entityJson.ToObject<T>(JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() }));
            return deserializeObject;
        }

        private IDictionary<string, object> CreateDynamicObject(string[] pathSplitted, Dictionary<string, object> dictionary, JObject jObject)
        {
            if (pathSplitted.Length == 1)
            {
                var dic = new Dictionary<string, object>();
                dic.Add(pathSplitted[0], jObject);
                return dic;
            }

            var dynamicObject = CreateDynamicObject(pathSplitted.Skip(1).ToArray(), dictionary, jObject);
            dictionary.Add(pathSplitted[0], dynamicObject);
            return dictionary;
        }

        public async Task<EventStoreResult<IEnumerable<DomainEvent>>> GetEvents(Guid entityId = default(Guid),
            int from = 0, int to = 100)
        {
            var streamEventsSlice = await GetStreamEventsSlice(entityId, from, to);
            var domainEvents = streamEventsSlice.Events.ToList();
            while (!streamEventsSlice.IsEndOfStream)
            {
                from = to + 1;
                to += 100;
                streamEventsSlice = await GetStreamEventsSlice(entityId, from, to);
                domainEvents.AddRange(streamEventsSlice.Events);
            }

            var eventEventNumber = streamEventsSlice.Events.Last().Event.EventNumber;
            return EventStoreResult<IEnumerable<DomainEvent>>.Ok(ToDomainEventList(domainEvents), eventEventNumber);
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainResultDomainEvents)
        {
            await AppendAsync(domainResultDomainEvents, ExpectedVersion.Any);
        }

        public async Task Subscribe(Type domainEventType, Action<DomainEvent> subscribeMethod)
        {
            await _eventStoreConnection.SubscribeToStreamAsync(
                $"{_eventStoreConfig.EventStream}-{domainEventType.Name}", true,
                (arg1, arg2) =>
                {
                    var domainEvent = _eventConverter.Deserialize(arg2);
                    subscribeMethod.Invoke(domainEvent);
                });
        }

        public void SubscribeFrom(Type domainEventType, long version, Action<DomainEvent> subscribeMethod)
        {
            _eventStoreConnection.SubscribeToStreamFrom($"{_eventStoreConfig.EventStream}-{domainEventType.Name}",
                0,
                new CatchUpSubscriptionSettings(int.MaxValue, 100, false, true),
                (arg1, arg2) =>
                {
                    var domainEvent = _eventConverter.Deserialize(arg2);
                    subscribeMethod.Invoke(domainEvent);
                });
        }

        public IEventStoreFacade Include<T>(string nameOfProperty) where T : Entity
        {
            AlsoLoad.Add(nameOfProperty);
            return this;
        }

        public ICollection<string> AlsoLoad { get; private set; } = new Collection<string>();


        private async Task<StreamEventsSlice> GetStreamEventsSlice(Guid entityId, int from, int to)
        {
            StreamEventsSlice streamEventsSlice;
            if (entityId == default(Guid))
                streamEventsSlice =
                    await _eventStoreConnection.ReadStreamEventsForwardAsync(_eventStoreConfig.EventStream, from,
                        to, true);
            else
                streamEventsSlice =
                    await _eventStoreConnection.ReadStreamEventsForwardAsync(
                        $"{_eventStoreConfig.EntityStream}-{entityId}", from, to, true);
            return streamEventsSlice;
        }

        private IEnumerable<DomainEvent> ToDomainEventList(List<ResolvedEvent> events)
        {
            foreach (var resolvedEvent in events) yield return _eventConverter.Deserialize(resolvedEvent);
        }
    }
}