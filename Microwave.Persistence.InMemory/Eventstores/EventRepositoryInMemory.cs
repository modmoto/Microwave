using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class EventRepositoryInMemory : IEventRepository
    {
        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0)
        {
            throw new NotImplementedException();
        }

        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset))
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset
        tickSince = default(DateTimeOffset))
        {
            throw new NotImplementedException();
        }

        public Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType)
        {
            throw new NotImplementedException();
        }
    }
}