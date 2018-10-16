using System;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStoreSubscription
    {
        void SubscribeFrom(string domainEventType, long version, Func<DomainEvent, StreamVersion, Task> subscribeMethod);
    }

    public class StreamVersion
    {
        public long EventNumber { get; }

        public StreamVersion(long eventNumber)
        {
            EventNumber = eventNumber;
        }
    }
}