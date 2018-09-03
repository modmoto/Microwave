using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Framework.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEventSourcingStrategy _eventSourcingStrategy;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IEventStoreConfig _eventStoreConfig;

        public EventStore(IEventSourcingStrategy eventSourcingStrategy, IEventStoreConnection eventStoreConnection, IEventStoreConfig eventStoreConfig)
        {
            _eventSourcingStrategy = eventSourcingStrategy;
            _eventStoreConnection = eventStoreConnection;
            _eventStoreConfig = eventStoreConfig;
            _eventStoreConnection.ConnectAsync();
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var domainEventSerialized = JsonConvert.SerializeObject(domainEvent);
                await _eventStoreConnection.AppendToStreamAsync(_eventStoreConfig.EventStream, ExpectedVersion.Any,
                    new EventData(Guid.NewGuid(), nameof(domainEvent.GetType), true, Encoding.UTF8.GetBytes(domainEventSerialized),
                        null));
            }
        }

        public async Task AppendAsync(DomainEvent domainEvent)
        {
            var domainEventSerialized = JsonConvert.SerializeObject(domainEvent);
            await _eventStoreConnection.AppendToStreamAsync(_eventStoreConfig.EventStream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), nameof(domainEvent.GetType), true, Encoding.UTF8.GetBytes(domainEventSerialized),
                    null));
        }

        public async Task<T> LoadAsync<T>(Guid commandEntityId) where T : new()
        {
            var events = await GetEvents();
            var entity = new T();
            var domainEventsForEntity = events.Where(domainEvent => domainEvent.EntityId == commandEntityId);
            foreach (var domainEvent in domainEventsForEntity)
            {
                entity = _eventSourcingStrategy.Apply(entity, domainEvent);
            }

            return entity;
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents(int from = 0, int to = 100)
        {
            var streamEventsSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync(_eventStoreConfig.EventStream, from, to, true);
            if (streamEventsSlice.IsEndOfStream) return ToDomainEventList(streamEventsSlice.Events);
            var domainEvents = await GetEvents(to + 1, to + 101);
            var domainEventsTemp = domainEvents.ToList();
            domainEventsTemp.AddRange(ToDomainEventList(streamEventsSlice.Events));
            return domainEventsTemp;

        }

        private IEnumerable<DomainEvent> ToDomainEventList(ResolvedEvent[] events)
        {
            foreach (var resolvedEvent in events)
            {
                var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var deserializeObject = JsonConvert.DeserializeObject<DomainEvent>(eventData);
                yield return deserializeObject;
            }
        }
    }

    public interface IEventStoreConfig
    {
        string EventStream { get; set; }
    }
}