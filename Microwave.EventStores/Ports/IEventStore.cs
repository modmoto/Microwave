using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IEventStore
    {
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<Result> AppendAsync(IDomainEvent domainEvent, long entityVersion);
        Task<Result<EventStoreResult<T>>> LoadAsync<T>(Identity entityId) where T : IApply, new();
    }
}