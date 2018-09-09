using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class EventStoreFacade : IEventStoreFacade
    {
        private readonly IEventSourcingStrategy _eventSourcingStrategy;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly EventStoreConfig _eventStoreConfig;
        private readonly IDomainEventConverter _eventConverter;

        public EventStoreFacade(IEventSourcingStrategy eventSourcingStrategy,
            IEventStoreConnection eventStoreConnection, EventStoreConfig eventStoreConfig, IDomainEventConverter eventConverter)
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

        public async Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId) where T : new()
        {
            var events = await GetEvents(commandEntityId);
            var entity = new T();
            foreach (var domainEvent in events.Result) entity = _eventSourcingStrategy.Apply(entity, domainEvent);

            return EventStoreResult<T>.Ok(entity, events.EntityVersion);
        }

        public async Task<EventStoreResult<IEnumerable<DomainEvent>>> GetEvents(Guid entityId = default(Guid), int from = 0, int to = 100)
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

            return EventStoreResult<IEnumerable<DomainEvent>>.Ok(ToDomainEventList(domainEvents), streamEventsSlice.LastEventNumber);
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainResultDomainEvents)
        {
            await AppendAsync(domainResultDomainEvents, ExpectedVersion.Any);
        }

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