using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public class EventRepository : IEventRepository
    {
        private readonly IDomainEventConverter _eventConverter;
        private readonly EventStoreContext _eventStoreContext;

        public EventRepository(IDomainEventConverter eventConverter, EventStoreContext eventStoreContext)
        {
            _eventConverter = eventConverter;
            _eventStoreContext = eventStoreContext;
        }

        public async Task<IEnumerable<DomainEvent>> LoadEventsByEntity(Guid entityId, long from = 0)
        {
            var stream =
                await _eventStoreContext.EntityStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                    str.EntityId == entityId);
            if (stream == null) return new List<DomainEvent>();
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    return domainEvent;
                });
            return domainEvents;
        }

        public async Task<IEnumerable<DomainEvent>> LoadEventsByTypeAsync(string domainEventTypeName, long from = 0)
        {
            var stream =
                await _eventStoreContext.TypeStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                    str.DomainEventType == domainEventTypeName);
            if (stream == null) return new List<DomainEvent>();
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    return domainEvent;
                });
            return domainEvents;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            // TODO do this asynchronosly with eventhandler
            var entityVersionTemp = entityVersion;
            foreach (var domainEvent in domainEvents)
            {
                entityVersionTemp++;
                var domainEventDbo = new DomainEventDbo
                {
                    Payload = _eventConverter.Serialize(domainEvent),
                    Version = entityVersionTemp
                };

                var typeStream = _eventStoreContext.TypeStreams.Include(st => st.DomainEvents)
                    .FirstOrDefault(st => st.DomainEventType == domainEvent.GetType().Name);
                var version = -1L;
                if (typeStream != null) version = typeStream.DomainEvents.Select(de => de.Version).Max();
                var domainEventTypeDbo = new DomainEventTypeDbo
                {
                    Payload = _eventConverter.Serialize(domainEvent),
                    Version = version + 1
                };

                var entityStream = _eventStoreContext.EntityStreams.Include(st => st.DomainEvents)
                    .FirstOrDefault(str => str.EntityId == domainEvent.EntityId);

                AddToTypeStream(typeStream, domainEventTypeDbo, domainEvent);
                AddToEntityStream(entityStream, domainEventDbo, domainEvent);
            }

            await _eventStoreContext.SaveChangesAsync();
        }

        private void AddToTypeStream(TypeStream typeStream, DomainEventTypeDbo domainEventDbo, DomainEvent domainEvent)
        {
            if (typeStream == null)
            {
                var stream = new TypeStream
                {
                    DomainEvents = new List<DomainEventTypeDbo> {domainEventDbo},
                    DomainEventType = domainEvent.GetType().Name
                };
                _eventStoreContext.TypeStreams.Add(stream);
            }
            else
            {
                typeStream.DomainEvents.Add(domainEventDbo);
            }
        }

        private void AddToEntityStream(EntityStream entityStream, DomainEventDbo domainEventDbo,
            DomainEvent domainEvent)
        {
            if (entityStream == null)
            {
                var stream = new EntityStream
                {
                    DomainEvents = new List<DomainEventDbo> {domainEventDbo},
                    EntityId = domainEvent.EntityId
                };
                _eventStoreContext.EntityStreams.Add(stream);
            }
            else
            {
                entityStream.DomainEvents.Add(domainEventDbo);
            }
        }
    }
}