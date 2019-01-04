using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public class EventStoreResult<T> : Result<T>
    {
        private readonly long _version;

        public long Version
        {
            get
            {
                Status.Check();
                return _version;
            }
        }

        protected EventStoreResult(T value, long version, ResultStatus state) : base(state)
        {
            _version = version;
            Value = value;
        }

        public new static EventStoreResult<T> NotFound(Identity notFoundId)
        {
            return new NotFoundEventStoreResult<T>(notFoundId);
        }

        public static EventStoreResult<T> Ok(T value, long version)
        {
            return new EventStoreResult<T>(value, version, new Ok());
        }
    }

    public class NotFoundEventStoreResult<T> : EventStoreResult<T>
    {
        public NotFoundEventStoreResult(Identity notFoundId) : base(default(T), 0, new NotFound(typeof(T), notFoundId))
        {
        }
    }
}