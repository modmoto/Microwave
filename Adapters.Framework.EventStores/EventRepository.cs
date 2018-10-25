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

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = 0)
        {
            var stream =
                await _eventStoreContext.EntityStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                    str.EntityId == entityId);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.DomainEvent.Payload);
                    domainEvent.Version = dbo.Version;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByTypeAsync(string domainEventTypeName, long from = 0)
        {
            var stream =
                await _eventStoreContext.TypeStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                    str.DomainEventType == domainEventTypeName);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.DomainEvent.Payload);
                    domainEvent.Version = dbo.Version;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var entityVersionTemp = entityVersion;
            foreach (var domainEvent in domainEvents)
            {
                entityVersionTemp++;
                var domainEventWrapper = new DomainEventWrapper
                {
                    DomainEvent = new DomainEventDbo
                    {
                        Payload = _eventConverter.Serialize(domainEvent)
                    },
                    Version = entityVersionTemp
                };

                await _eventStoreContext.DomainEvents.AddAsync(domainEventWrapper);
            }

            await _eventStoreContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}