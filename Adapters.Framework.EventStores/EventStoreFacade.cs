using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;
using EventStore.ClientAPI;

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
            var eventList = domainEvents.ToList();
            var firstEvent = eventList.FirstOrDefault();
            if (firstEvent != null)
            {
                if (eventList.Any(ev => ev.EntityId != firstEvent.EntityId))
                    throw new ArgumentException(
                        "Entity Ids have to be the same, can not write to two or more streams with optimistic concurrency");
                var convertedElements = eventList.Select(eve => new EventData(eve.DomainEventId, eve.GetType().Name, true,
                    Encoding.UTF8.GetBytes(_eventConverter.Serialize(eve)), null));
                await _eventStoreConnection.AppendToStreamAsync(
                    $"{_eventStoreConfig.WriteStream}-{firstEvent.EntityId}", entityVersion,
                    convertedElements);
            }
        }

        public async Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId) where T : new()
        {
            var events = await GetEvents(commandEntityId);
            var entity = new T();
            foreach (var domainEvent in events.Result) entity = _eventSourcingStrategy.Apply(entity, domainEvent);

            return EventStoreResult<T>.Ok(entity, events.EntityVersion);
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
}