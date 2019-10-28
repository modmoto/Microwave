using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;

namespace Microwave.EventStores
{
    public interface IEventStore
    {
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<Result> AppendAsync(IDomainEvent domainEvent, long entityVersion);
        Task<EventStoreResult<T>> LoadAsync<T>(string entityId) where T : IApply, new();
    }
}