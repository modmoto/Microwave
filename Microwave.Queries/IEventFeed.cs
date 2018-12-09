using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application;

namespace Microwave.Queries
{
    public interface IEventFeed
    {
        Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion);
    }
}