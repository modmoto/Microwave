using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EntityStreamRepository : IEntityStreamRepository
    {
        private readonly IObjectConverter _eventConverter;
        private readonly EventStoreWriteContext _eventStoreWriteContext;

        public EntityStreamRepository(IObjectConverter eventConverter, EventStoreWriteContext eventStoreWriteContext)
        {
            _eventConverter = eventConverter;
            _eventStoreWriteContext = eventStoreWriteContext;
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString() && str.Version > from).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEvent>>.NotFound(entityId.ToString());

            var domainEvents = stream.Select(dbo => _eventConverter.Deserialize<DomainEvent>(dbo.Payload));

            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsSince(long tickSince = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.Created > tickSince).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());

            var domainEvents = stream.Select(dbo => _eventConverter.Deserialize<DomainEvent>(dbo.Payload));

            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString()).ToList();

            var entityVersionTemp = stream.Count - 1;
            if (entityVersionTemp != entityVersion) return Result.ConcurrencyResult(entityVersion, entityVersionTemp);

            foreach (var domainEvent in events)
            {
                entityVersionTemp = entityVersionTemp + 1;
                domainEvent.MarkNow(entityVersionTemp);
                var serialize = _eventConverter.Serialize(domainEvent);
                var domainEventDbo = new DomainEventDbo
                {
                    Payload = serialize,
                    Created = domainEvent.Created,
                    Version = domainEvent.Version,
                    EntityId = domainEvent.EntityId.ToString()
                };

                _eventStoreWriteContext.EntityStreams.Add(domainEventDbo);
            }

            await _eventStoreWriteContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}