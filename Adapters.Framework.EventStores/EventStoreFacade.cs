using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            if (entityVersion == 0) entityVersion = -1;
            var groupBy = domainEvents.GroupBy(domainEvent => domainEvent.EntityId);
            foreach (var result in groupBy)
            {
                var convertedElements = result.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                    Encoding.UTF8.GetBytes(_eventConverter.Serialize(eve)), null));
                await _eventStoreConnection.AppendToStreamAsync($"{_eventStoreConfig.WriteStream}-{result.Key}", entityVersion,
                    convertedElements);
            }
        }

        public async Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId) where T : new()
        {
            var eventStoreResult = await LoadAsyncRecursive<T>(commandEntityId, AlsoLoad);
            AlsoLoad = new List<string>();
            AlsoLoadChilds = new List<ParentChild>();
            return eventStoreResult;
        }

        public async Task<EventStoreResult<T>> LoadAsyncRecursive<T>(Guid commandEntityId, IEnumerable<string> list) where T : new()
        {
            var events = await GetEvents(commandEntityId);
            var entity = new T();
            foreach (var domainEvent in events.Result) entity = _eventSourcingStrategy.Apply(entity, domainEvent);

            var type = typeof(T);

            foreach (var loadKey in list)
            {
                var propertyInfos = type.GetProperty(loadKey);
                var entityType = propertyInfos.PropertyType;
                var methodInfo = GetType().GetMethod("LoadAsyncRecursive");
                var makeGenericMethod = methodInfo.MakeGenericMethod(entityType);

                var guid = ((Entity) propertyInfos.GetValue(entity)).Id;

                var listNew = AlsoLoadChilds.Where(item => item.NameOfParent == loadKey).Select(item => item.NameOfChild);
                var invoke = (Task) makeGenericMethod.Invoke(this, new object[]{ guid, listNew });
                await invoke;
                var propertyInfo = invoke.GetType().GetProperty("Result");
                var value = propertyInfo.GetValue(invoke);
                var ent = value.GetType().GetProperty("Result").GetValue(value);
                entity = Merge(entity, ent, loadKey);
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

        public async Task Subscribe(Type domainEventType, Action<DomainEvent> subscribeMethod)
        {
            await _eventStoreConnection.SubscribeToStreamAsync(
                $"{_eventStoreConfig.ReadStream}-{domainEventType.Name}", true,
                (arg1, arg2) =>
                {
                    var domainEvent = _eventConverter.Deserialize(arg2);
                    subscribeMethod.Invoke(domainEvent);
                });
        }

        public void SubscribeFrom(Type domainEventType, long version, Action<DomainEvent> subscribeMethod)
        {
            _eventStoreConnection.SubscribeToStreamFrom($"{_eventStoreConfig.ReadStream}-{domainEventType.Name}",
                0,
                new CatchUpSubscriptionSettings(int.MaxValue, 100, false, true),
                (arg1, arg2) =>
                {
                    var domainEvent = _eventConverter.Deserialize(arg2);
                    subscribeMethod.Invoke(domainEvent);
                });
        }

        public IEventStoreFacade Include(string nameOfProperty)
        {
            AlsoLoad.Add(nameOfProperty);
            return this;
        }

        public IEventStoreFacade FurtherInclude(string nameOfParent, string nameOfChild)
        {
            AlsoLoadChilds.Add(new ParentChild(nameOfParent, nameOfChild));
            return this;
        }

        public ICollection<string> AlsoLoad { get; private set; } = new Collection<string>();
        public ICollection<ParentChild> AlsoLoadChilds { get; private set; } = new Collection<ParentChild>();


        private async Task<StreamEventsSlice> GetStreamEventsSlice(Guid entityId, int from, int to)
        {
            StreamEventsSlice streamEventsSlice;
            if (entityId == default(Guid))
                streamEventsSlice =
                    await _eventStoreConnection.ReadStreamEventsForwardAsync(_eventStoreConfig.ReadStream, from,
                        to, true);
            else
                streamEventsSlice =
                    await _eventStoreConnection.ReadStreamEventsForwardAsync(
                        $"{_eventStoreConfig.WriteStream}-{entityId}", from, to, true);
            return streamEventsSlice;
        }

        private IEnumerable<DomainEvent> ToDomainEventList(List<ResolvedEvent> events)
        {
            foreach (var resolvedEvent in events) yield return _eventConverter.Deserialize(resolvedEvent);
        }
    }

    public class ParentChild
    {
        public string NameOfParent { get; }
        public string NameOfChild { get; }

        public ParentChild(string nameOfParent, string nameOfChild)
        {
            NameOfParent = nameOfParent;
            NameOfChild = nameOfChild;
        }
    }
}