using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Domain.Results;

namespace Microwave.EventStores
{
    public interface IEventRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0);
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset));
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince = default(DateTimeOffset));
        Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType);
    }
}