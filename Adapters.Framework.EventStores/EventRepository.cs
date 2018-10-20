using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EventRepository : IEventRepository
    {
        IEnumerable<DomainEvent> _domainEvents = new List<DomainEvent>();

        public async Task<IEnumerable<DomainEvent>> LoadEvents(Guid entityId)
        {
            return _domainEvents.Where(domaintEvent => domaintEvent.EntityId == entityId);
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            foreach (var domainEvent in domainEvents)
            {
                _domainEvents.Append(domainEvent);
            }
        }
    }
}