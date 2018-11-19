using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : new ();
    }

    public class EventstoreResult<T>
    {
        public EventstoreResult(long version, T value)
        {
            Version = version;
            Value = value;
        }

        public long Version { get; }
        public T Value { get; }
    }
}