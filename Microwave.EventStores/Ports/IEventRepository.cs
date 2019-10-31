using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;

namespace Microwave.EventStores.Ports
{
    public interface IEventRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long lastVersion = 0);
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long lastVersion = 0);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long lastVersion = 0);
    }
}