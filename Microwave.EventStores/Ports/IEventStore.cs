using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<EventstoreResult<T>> LoadAsync<T>(Identity entityId) where T : IApply, new();
    }

    public class EventstoreResult<T>
    {
        public EventstoreResult(long version, T entity)
        {
            Version = version;
            Entity = entity;
        }

        public long Version { get; }
        public T Entity { get; }
    }
}