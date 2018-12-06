using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.Application.Ports
{
    public interface IEventFeed<T> where T : IDomainEvent
    {
        Task<IEnumerable<DomainEventHto<T>>> GetEventsAsync(long lastVersion);
    }
}