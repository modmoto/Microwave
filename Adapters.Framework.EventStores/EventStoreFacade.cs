using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Framework.EventStores
{
    public class EventStoreFacade : IEventStore
    {
        private readonly IEventSourcingStrategy _eventSourcingStrategy;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly EventStoreConfig _realEventStoreConfig;

        public EventStoreFacade(IEventSourcingStrategy eventSourcingStrategy, IEventStoreConnection eventStoreConnection, EventStoreConfig realEventStoreConfig)
        {
            _eventSourcingStrategy = eventSourcingStrategy;
            _eventStoreConnection = eventStoreConnection;
            _realEventStoreConfig = realEventStoreConfig;
            _eventStoreConnection.ConnectAsync();
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateSetterContractResolver() };
            var convertedElements = domainEvents.Select(eve => new EventData(Guid.NewGuid(), nameof(eve.GetType), true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eve, settings)), null));
            await _eventStoreConnection.AppendToStreamAsync(_realEventStoreConfig.EventStream, ExpectedVersion.Any,
                    convertedElements);
        }

        public async Task AppendAsync(DomainEvent domainEvent)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateSetterContractResolver() };
            var domainEventSerialized = JsonConvert.SerializeObject(domainEvent, settings);
            await _eventStoreConnection.AppendToStreamAsync(_realEventStoreConfig.EventStream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), nameof(domainEvent.GetType), true, Encoding.UTF8.GetBytes(domainEventSerialized),
                    null));
        }

        public async Task<T> LoadAsync<T>(Guid commandEntityId) where T : new()
        {
            var events = await GetEvents(commandEntityId);
            var entity = new T();
            foreach (var domainEvent in events)
            {
                entity = _eventSourcingStrategy.Apply(entity, domainEvent);
            }

            return entity;
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents(Guid entityId = default(Guid), int from = 0, int to = 100)
        {
            StreamEventsSlice streamEventsSlice;
            if (entityId == default(Guid))
            {
                streamEventsSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync($"{_realEventStoreConfig.EntityStream}-{entityId}", from, to, true);
            }
            else
            {
                streamEventsSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync(_realEventStoreConfig.EventStream, from, to, true);
            }

            if (streamEventsSlice.IsEndOfStream) return ToDomainEventList(streamEventsSlice.Events);
            var domainEvents = await GetEvents(entityId, to + 1, to + 101);
            var domainEventsTemp = domainEvents.ToList();
            domainEventsTemp.AddRange(ToDomainEventList(streamEventsSlice.Events));
            return domainEventsTemp;

        }

        private IEnumerable<DomainEvent> ToDomainEventList(ResolvedEvent[] events)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateSetterContractResolver() };
            foreach (var resolvedEvent in events)
            {
                var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var deserializeObject = JsonConvert.DeserializeObject<DomainEvent>(eventData, settings);
                yield return deserializeObject;
            }
        }
    }
}