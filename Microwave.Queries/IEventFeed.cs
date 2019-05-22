using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IEventFeed<T>
    {
        Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset));
    }
}