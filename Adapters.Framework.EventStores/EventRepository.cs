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
        private readonly DomainEventConverter _eventConverter;
        private readonly EventStoreContext _eventStoreContext;

        public EventRepository(DomainEventConverter eventConverter, EventStoreContext eventStoreContext)
        {
            _eventConverter = eventConverter;
            _eventStoreContext = eventStoreContext;
        }

        public async Task<IEnumerable<DomainEvent>> LoadEventsByEntity(Guid entityId, long from = 0)
        {
            var stream =
                await _eventStoreContext.EntityStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                    str.EntityId == entityId && BitConverter.ToInt64(str.Version) > from);
            if (stream == null) return new List<DomainEvent>();
            var domainEvents = stream.DomainEvents.Select(dbo => _eventConverter.Deserialize(dbo.Payload));
            return domainEvents;
        }

        public async Task<IEnumerable<DomainEvent>> LoadEventsByType(string domainEventTypeName, long from = 0)
        {
            var stream = await _eventStoreContext.TypeStreams.Include(ev => ev.DomainEvents).FirstOrDefaultAsync(str =>
                str.DomainEventType == domainEventTypeName && BitConverter.ToInt64(str.Version) > from);
            if (stream == null) return new List<DomainEvent>();
            var domainEvents = stream.DomainEvents.Select(dbo => _eventConverter.Deserialize(dbo.Payload));
            return domainEvents;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            foreach (var domainEvent in domainEvents)
            {
                var domainEventDbo = new DomainEventDbo
                {
                    Payload = _eventConverter.Serialize(domainEvent),
                };

                var typeStream = _eventStoreContext.TypeStreams.Include(st => st.DomainEvents).FirstOrDefault(str => str.DomainEventType == domainEvent.GetType().Name);
                var entityStream = _eventStoreContext.EntityStreams.Include(st => st.DomainEvents).FirstOrDefault(str => str.EntityId == domainEvent.EntityId);

                AddToTypeStream(typeStream, domainEventDbo, domainEvent);
                AddToEntityStream(entityStream, domainEventDbo, domainEvent);
            }

            var typeStreams = _eventStoreContext.TypeStreams.ToList();
            await _eventStoreContext.SaveChangesAsync();
        }

        private void AddToTypeStream(TypeStream typeStream, DomainEventDbo domainEventDbo, DomainEvent domainEvent)
        {
            if (typeStream == null)
            {
                var stream = new TypeStream
                {
                    DomainEvents = new List<DomainEventDbo> {domainEventDbo},
                    DomainEventType = domainEvent.GetType().Name
                };
                _eventStoreContext.TypeStreams.Add(stream);
            }
            else
            {
                typeStream.DomainEvents.Add(domainEventDbo);
            }
        }

        private void AddToEntityStream(EntityStream entityStream, DomainEventDbo domainEventDbo, DomainEvent domainEvent)
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