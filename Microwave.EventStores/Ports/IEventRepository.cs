using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;

namespace Microwave.EventStores.Ports
{
    public interface IEventRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long from = 0);
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset));
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince = default(DateTimeOffset));
    }
}