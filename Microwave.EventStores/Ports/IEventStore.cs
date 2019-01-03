using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Exceptions;
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
        private readonly T _entity;
        private readonly long _version;

        public EventstoreResult()
        {
        }

        public EventstoreResult(long version, T entity)
        {
            _version = version;
            _entity = entity;
        }

        public long Version =>
            this is EventstoreResultNotFound<T> res
                ? throw new NotFoundException(typeof(T), res.EntityId.Id)
                : _version;

        public T Entity =>
            this is EventstoreResultNotFound<T> res
                ? throw new NotFoundException(typeof(T), res.EntityId.Id)
                : _entity;

        public static EventstoreResult<T> NotFound(Identity entityId)
        {
            return new EventstoreResultNotFound<T>(entityId);
        }
    }

    internal class EventstoreResultNotFound<T> : EventstoreResult<T>
    {
        public EventstoreResultNotFound(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}