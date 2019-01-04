using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<EventstoreResult<T>> LoadAsync<T>(Identity entityId) where T : IApply, new();
    }

    public class EventstoreResult<T> : Result<T>
    {
        private long _version;

        public long Version
        {
            get
            {
                Status.Check();
                return _version;
            }
        }

        protected EventstoreResult(T value, long version, ResultStatus state) : base(state)
        {
            _version = version;
            Value = value;
        }

        public static new EventstoreResult<T> NotFound(Identity notFoundId)
        {
            return new NotFoundEventstoreResult<T>(notFoundId);
        }

        public static new EventstoreResult<T> Ok(T value, long version)
        {
            return new EventstoreResult<T>(value, version, new Ok());
        }
    }

    public class NotFoundEventstoreResult<T> : EventstoreResult<T>
    {
        public NotFoundEventstoreResult(Identity notFoundId) : base(default(T), 0, new NotFound(typeof(T), notFoundId))
        {
        }
    }
}