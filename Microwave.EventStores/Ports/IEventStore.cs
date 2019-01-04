using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<EventStoreResult<T>> LoadAsync<T>(Identity entityId) where T : IApply, new();
    }
}