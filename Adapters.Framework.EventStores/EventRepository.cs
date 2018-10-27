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
        private readonly EventStoreContext _eventStoreContext;

        public EventRepository(IObjectConverter eventConverter, EventStoreContext eventStoreContext)
        {
            _eventConverter = eventConverter;
            _eventStoreContext = eventStoreContext;
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = -1)
        {
            var stream =
                await _eventStoreContext.EntityStreams
                    .Include(ev => ev.DomainEvents)
                    .ThenInclude(ev => ev.DomainEvent)
                    .FirstOrDefaultAsync(str => str.EntityId == entityId);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.DomainEvent.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.DomainEvent.Created;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByTypeAsync(string domainEventTypeName,
            long from = -1)
        {
            var stream =
                await _eventStoreContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .ThenInclude(ev => ev.DomainEvent)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEventTypeName);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.DomainEvent.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.DomainEvent.Created;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsSince(long tickSince = -1)
        {
            var domainEventWrappers = await _eventStoreContext.EntityStreams
                .Include(s => s.DomainEvents)
                .ThenInclude(e => e.DomainEvent)
                .ToListAsync();
            var eventWrappers = domainEventWrappers
                .SelectMany(stream => stream.DomainEvents
                    .Where(ev => ev.DomainEvent.Created > tickSince));

            var loadEventsSince = eventWrappers
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.DomainEvent.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.DomainEvent.Created;
                    return domainEvent;
                });

            return Result<IEnumerable<DomainEvent>>.Ok(loadEventsSince);
        }

        public async Task<Result> AppendToOverallStream(IEnumerable<DomainEvent> events)
        {
            var allStream = _eventStoreContext.TypeStreams.Include(e => e.DomainEvents)
                .ThenInclude(e => e.DomainEvent).FirstOrDefault(d => d.DomainEventType == "AllDomainEventsStream");

            if (allStream == null)
            {
                allStream = new TypeStream
                {
                    DomainEvents = new List<DomainEventWrapper>(),
                    DomainEventType = "AllDomainEventsStream",
                    Version = -1
                };

                _eventStoreContext.TypeStreams.Add(allStream);
            }

            var entityVersionTemp = allStream.Version;
            foreach (var domainEvent in events)
            {
                entityVersionTemp++;
                var domainEventWrapper = new DomainEventWrapper
                {
                    // Todo link with other domain events
                    DomainEvent = new DomainEventDbo
                    {
                        Payload = _eventConverter.Serialize(domainEvent),
                        Created = DateTimeOffset.UtcNow.Ticks
                    },
                    Version = entityVersionTemp
                };

                allStream.DomainEvents.Add(domainEventWrapper);
            }

            allStream.Version = entityVersionTemp;
            await _eventStoreContext.SaveChangesAsync();
            return Result.Ok();
        }

        public Task<Result<IEnumerable<DomainEvent>>> LoadOverallStream(long from = -1)
        {
            return LoadEventsByTypeAsync("AllDomainEventsStream", from);
        }

        public async Task<Result> AppendToTypeStream(DomainEvent domainEvent)
        {
            var typeStream =
                await _eventStoreContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .ThenInclude(ev => ev.DomainEvent)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEvent.GetType().Name);

            if (typeStream == null)
            {
                typeStream = new TypeStream
                {
                    DomainEvents = new List<DomainEventWrapper>(),
                    DomainEventType = domainEvent.GetType().Name,
                    Version = -1
                };

                _eventStoreContext.TypeStreams.Add(typeStream);
            }

            var entityVersionTemp = typeStream.Version + 1;

            var domainEventWrapper = new DomainEventWrapper
            {
                // Todo link with other domain events
                DomainEvent = new DomainEventDbo
                {
                    Payload = _eventConverter.Serialize(domainEvent),
                    Created = DateTimeOffset.UtcNow.Ticks
                },
                Version = entityVersionTemp
            };


            typeStream.Version = entityVersionTemp;
            typeStream.DomainEvents.Add(domainEventWrapper);

            await _eventStoreContext.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            var entityStream = await _eventStoreContext.EntityStreams.FindAsync(entityId);

            if (entityStream == null)
            {
                entityStream = new EntityStream
                {
                    EntityId = entityId,
                    DomainEvents = new List<DomainEventWrapper>(),
                    Version = -1L
                };
                _eventStoreContext.EntityStreams.Add(entityStream);
            }

            if (entityStream.Version != entityVersion)
                return Result.ConcurrencyResult(entityStream.Version, entityVersion);

            var entityVersionTemp = entityVersion;
            foreach (var domainEvent in events)
            {
                entityVersionTemp++;
                var domainEventWrapper = new DomainEventWrapper
                {
                    DomainEvent = new DomainEventDbo
                    {
                        Payload = _eventConverter.Serialize(domainEvent),
                        Created = DateTimeOffset.UtcNow.Ticks
                    },
                    Version = entityVersionTemp
                };

                entityStream.DomainEvents.Add(domainEventWrapper);
            }

            entityStream.Version += events.Count;

            await _eventStoreContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}