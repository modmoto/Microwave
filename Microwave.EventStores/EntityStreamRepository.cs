using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.ObjectPersistences;

namespace Microwave.EventStores
{
    public class EntityStreamRepository : IEntityStreamRepository
    {
        private readonly DomainEventDeserializer _domainEventConverter;
        private readonly EventStoreWriteContext _eventStoreWriteContext;
        private readonly IObjectConverter _converter;
        private readonly object _lock = new Object();

        public EntityStreamRepository(
            DomainEventDeserializer domainEventConverter,
            EventStoreWriteContext eventStoreWriteContext,
            IObjectConverter converter)
        {
            _domainEventConverter = domainEventConverter;
            _eventStoreWriteContext = eventStoreWriteContext;
            _converter = converter;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Guid entityId, long from = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString() && str.Version > from).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId.ToString());

            var domainEvents = stream.Select(dbo =>
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

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsSince(long tickSince = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.Created > tickSince).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

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

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        //TODO remove Lock and make threadsafe
        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            lock (_lock)
            {
                var stream = _eventStoreWriteContext.EntityStreams
                    .Where(str => str.EntityId == entityId.ToString()).ToList();

                var entityVersionTemp = stream.LastOrDefault()?.Version ?? -1;
                if (entityVersionTemp != entityVersion) return Result.ConcurrencyResult(entityVersion, entityVersionTemp);

                foreach (var domainEvent in events)
                {
                    entityVersionTemp = entityVersionTemp + 1;
                    var serialize = _converter.Serialize(domainEvent);
                    var domainEventDbo = new DomainEventDbo
                    {
                        Payload = serialize,
                        Created = DateTime.UtcNow.Ticks,
                        Version = entityVersionTemp,
                        EntityId = domainEvent.EntityId.ToString()
                    };

                    _eventStoreWriteContext.EntityStreams.Add(domainEventDbo);
                }

                _eventStoreWriteContext.SaveChanges();
                return Result.Ok();
            }
        }
    }
}