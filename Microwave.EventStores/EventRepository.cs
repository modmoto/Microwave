using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Application;
using Microwave.Application.Ports;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.ObjectPersistences;

namespace Microwave.EventStores
{
    public class EventRepository : IEventRepository
    {
        private readonly DomainEventDeserializer _domainEventConverter;
        private readonly EventStoreContext _eventStoreContext;
        private readonly IObjectConverter _converter;
        private readonly object _lock = new Object();

        public EventRepository(
            DomainEventDeserializer domainEventConverter,
            EventStoreContext eventStoreContext,
            IObjectConverter converter)
        {
            _domainEventConverter = domainEventConverter;
            _eventStoreContext = eventStoreContext;
            _converter = converter;
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Guid entityId, long from = 0)
        {
            var stream = _eventStoreContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString() && str.Version > from).ToList();
            if (!stream.Any()) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId.ToString()));

            var domainEvents = stream.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Version,
                    DomainEvent = _domainEventConverter.Deserialize(dbo.Payload)
                };
            });

            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents));
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long tickSince = 0)
        {
            var domainEventDbos = _eventStoreContext.EntityStreams.ToList();
            var stream = domainEventDbos
                .Where(str => str.Created > tickSince).ToList();
            if (!stream.Any()) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>()));

            var domainEvents = stream.Select(dbo =>
            {
                var domainEvent = _domainEventConverter.Deserialize(dbo.Payload);
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Version,
                    DomainEvent = domainEvent
                };
            });

            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents));
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long version)
        {
            var domainEventTypeDbos = await _eventStoreContext.EntityStreams
                .Where(eventDbo => eventDbo.EventType == eventType && eventDbo.Created > version).ToListAsync();

            if (!domainEventTypeDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventTypeDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Version,
                    DomainEvent = _domainEventConverter.Deserialize(dbo.Payload)
                };
            });
            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        //TODO remove Lock and make threadsafe
        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            if (!events.Any()) return Task.FromResult(Result.Ok());
            var entityId = events.First().EntityId;
            lock (_lock)
            {
                var stream = _eventStoreContext.EntityStreams
                    .Where(str => str.EntityId == entityId.ToString()).ToList();

                var lastOrDefault = stream.LastOrDefault();
                var entityVersionTemp = lastOrDefault?.Version ?? 0;
                if (entityVersionTemp != entityVersion) return Task.FromResult(Result.ConcurrencyResult(entityVersion, entityVersionTemp));

                foreach (var domainEvent in events)
                {
                    entityVersionTemp = entityVersionTemp + 1;
                    var serialize = _converter.Serialize(domainEvent);
                    var domainEventDbo = new DomainEventDbo
                    {
                        Payload = serialize,
                        Created = DateTime.UtcNow.Ticks,
                        Version = entityVersionTemp,
                        EventType = domainEvent.GetType().Name,
                        EntityId = domainEvent.EntityId.ToString()
                    };

                    _eventStoreContext.EntityStreams.Add(domainEventDbo);
                }

                _eventStoreContext.SaveChanges();
                return Task.FromResult(Result.Ok());
            }
        }
    }
}