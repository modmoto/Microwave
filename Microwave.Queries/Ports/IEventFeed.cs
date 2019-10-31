using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Queries.Ports
{
    public interface IEventFeed<T>
    {
        Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0);
    }
}