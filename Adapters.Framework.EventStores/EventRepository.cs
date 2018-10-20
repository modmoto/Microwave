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

        public async Task<IEnumerable<DomainEvent>> LoadEvents(Guid entityId)
        {
            var domainEventDbos = await _eventStoreContext.DomainEvents.Where(domainEvent => domainEvent.EntityId == entityId)
                .ToListAsync();
            var domainEvents = domainEventDbos.Select(dbo => _eventConverter.Deserialize(dbo.Payload));
            return domainEvents;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var seializedEvents = domainEvents.Select(domainEvent =>
                new DomainEventDbo
                {
                    EntityId = domainEvent.EntityId,
                    Payload = _eventConverter.Serialize(domainEvent),
                    DomainEventType = domainEvent.GetType().Name
                });
            _eventStoreContext.DomainEvents.AddRange(seializedEvents);
            await _eventStoreContext.SaveChangesAsync();
        }
    }
}