using Microwave.Domain;
using Microwave.Domain.Results;

namespace Microwave.EventStores.Ports
{
    public class EventStoreResult<T> : Result<T>
    {
        protected EventStoreResult(ResultStatus status, T entity, long version) : base(status)
        {
            _version = version;
            _value = entity;
        }

        public EventStoreResult(T entity, long version) : base(new Ok())
        {
            _version = version;
            _value = entity;
        }

        public long Version
        {
            get
            {
                Status.Check();
                return _version;
            }
        }

        private readonly long _version;

        public static EventStoreResult<T> Ok(T value, long version)
        {
            return new Ok<T>(value, version);
        }

        public new static EventStoreResult<T> NotFound(Identity notFoundId)
        {
            return new NotFoundResult<T>(notFoundId);
        }
    }

    public class Ok<T> : EventStoreResult<T>
    {
        public Ok(T entity, long version) : base(new Ok(), entity, version)
        {
        }
    }

    public class NotFoundResult<T> : EventStoreResult<T>
    {
        public NotFoundResult(Identity notFoundId) : base(new NotFound(typeof(T), notFoundId), default(T), -1)
        {
        }
    }
}