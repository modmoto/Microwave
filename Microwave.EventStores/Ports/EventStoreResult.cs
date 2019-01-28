using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public class EventStoreResult<T>
    {
        protected EventStoreResult(ResultStatus status, T entity, long version)
        {
            _version = version;
            _entity = entity;
            Status = status;
        }

        public long Version
        {
            get
            {
                Status.Check();
                return _version;
            }
        }

        private T _entity;
        private readonly long _version;

        protected ResultStatus Status { get; }

        public T Entity
        {
            get
            {
                Status.Check();
                return _entity;
            }
        }

        public static EventStoreResult<T> Ok(T value, long version)
        {
            return new Ok<T>(value, version);
        }

        public bool Is<TCheck>() where TCheck : ResultStatus
        {
            return typeof(TCheck) == Status.GetType();
        }

        public static EventStoreResult<T> NotFound(Identity notFoundId)
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