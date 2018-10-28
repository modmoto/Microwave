using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;

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
            var stream =
                await _eventStoreWriteContext.EntityStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.EntityId == entityId.ToString());
            if (stream == null) return Result<IEnumerable<DomainEvent>>.NotFound(entityId.ToString());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    domainEvent.DomainEventId = new Guid(dbo.Id);
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsSince(long tickSince = -1)
        {
            var domainEventWrappers = await _eventStoreWriteContext.EntityStreams
                .Include(s => s.DomainEvents)
                .ToListAsync();
            var eventWrappers = domainEventWrappers
                .SelectMany(stream => stream.DomainEvents
                    .Where(ev => ev.Created > tickSince));

            var loadEventsSince = eventWrappers
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    domainEvent.DomainEventId = new Guid(dbo.Id);
                    return domainEvent;
                });

            return Result<IEnumerable<DomainEvent>>.Ok(loadEventsSince);
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            var entityStream = await _eventStoreWriteContext.EntityStreams.FindAsync(entityId.ToString());

            if (entityStream == null)
            {
                entityStream = new EntityStream
                {
                    EntityId = entityId.ToString(),
                    DomainEvents = new List<DomainEventDbo>(),
                    Version = -1L
                };
                _eventStoreWriteContext.EntityStreams.Add(entityStream);
            }

            if (entityStream.Version != entityVersion)
                return Result.ConcurrencyResult(entityStream.Version, entityVersion);

            var entityVersionTemp = entityVersion;
            foreach (var domainEvent in events)
            {
                entityVersionTemp++;
                var serialize = _eventConverter.Serialize(domainEvent);
                var domainEventDbo = new DomainEventDbo
                {
                    Id = domainEvent.DomainEventId.ToString(),
                    Payload = serialize,
                    Created = DateTimeOffset.UtcNow.Ticks,
                    Version = entityVersionTemp
                };

                entityStream.DomainEvents.Add(domainEventDbo);
            }

            entityStream.Version += events.Count;

            await _eventStoreWriteContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}