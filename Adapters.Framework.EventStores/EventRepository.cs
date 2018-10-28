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
    public class EventRepository : IEventRepository
    {
        private readonly IObjectConverter _eventConverter;
        private readonly EventStoreWriteContext _eventStoreWriteContext;
        private readonly EventStoreReadContext _readContext;

        public EventRepository(IObjectConverter eventConverter, EventStoreWriteContext eventStoreWriteContext, EventStoreReadContext readContext)
        {
            _eventConverter = eventConverter;
            _eventStoreWriteContext = eventStoreWriteContext;
            _readContext = readContext;
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = -1)
        {
            var stream =
                await _eventStoreWriteContext.EntityStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.EntityId == entityId);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    domainEvent.DomainEventId = dbo.Id;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByTypeAsync(string domainEventTypeName,
            long from = -1)
        {
            var stream =
                await _readContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEventTypeName);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    domainEvent.DomainEventId = dbo.Id;
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
                    domainEvent.DomainEventId = dbo.Id;
                    return domainEvent;
                });

            return Result<IEnumerable<DomainEvent>>.Ok(loadEventsSince);
        }

        public async Task<Result> AppendToOverallStream(IEnumerable<DomainEvent> events)
        {
            var allStream = _readContext.OverallStream;

            var entityVersionTemp = allStream.Count();
            foreach (var domainEvent in events)
            {
                var domainEventDbo = await CreateDomainEventCopyOverall(domainEvent);
                entityVersionTemp++;
                domainEventDbo.Version = entityVersionTemp;

                allStream.Add(domainEventDbo);
            }

            await _readContext.SaveChangesAsync();
            return Result.Ok();
        }

        private async Task<DomainEventDboCopy> CreateDomainEventCopy(DomainEvent domainEventWrapper)
        {
            var payLoad = _eventConverter.Serialize(domainEventWrapper);
            return new DomainEventDboCopy
            {
                Payload = payLoad,
                Created = domainEventWrapper.Created,
                Id = domainEventWrapper.DomainEventId,
                Version = domainEventWrapper.Version
            };
        }

        private async Task<DomainEventDboCopyOverallStream> CreateDomainEventCopyOverall(DomainEvent domainEventWrapper)
        {
            var payLoad = _eventConverter.Serialize(domainEventWrapper);
            return new DomainEventDboCopyOverallStream
            {
                Payload = payLoad,
                Created = domainEventWrapper.Created,
                Id = domainEventWrapper.DomainEventId,
                Version = domainEventWrapper.Version
            };
        }

        public Task<Result<IEnumerable<DomainEvent>>> LoadOverallStream(long from = -1)
        {
            return LoadEventsByTypeAsync("AllDomainEventsStream", from);
        }

        public async Task<Result> AppendToTypeStream(DomainEvent domainEvent)
        {
            var typeStream =
                await _readContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEvent.GetType().Name);

            if (typeStream == null)
            {
                typeStream = new TypeStream
                {
                    DomainEvents = new List<DomainEventDboCopy>(),
                    DomainEventType = domainEvent.GetType().Name,
                    Version = -1
                };

                _readContext.TypeStreams.Add(typeStream);
            }

            var entityVersionTemp = typeStream.Version + 1;

            var domainEventDbo = await CreateDomainEventCopy(domainEvent);

            domainEventDbo.Version = entityVersionTemp;

            typeStream.Version = entityVersionTemp;
            typeStream.DomainEvents.Add(domainEventDbo);

            await _readContext.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            var entityStream = await _eventStoreWriteContext.EntityStreams.FindAsync(entityId);

            if (entityStream == null)
            {
                entityStream = new EntityStream
                {
                    EntityId = entityId,
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
                    Id = domainEvent.DomainEventId,
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