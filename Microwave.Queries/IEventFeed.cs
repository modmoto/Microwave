using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IEventFeed<T> where T : IDomainEvent
    {
        Task<IEnumerable<DomainEventHto<T>>> GetEventsAsync(long lastVersion);
    }
}