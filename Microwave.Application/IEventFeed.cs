using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.Application
{
    public interface IEventFeed<T> where T : IDomainEvent
    {
        Task<IEnumerable<T>> GetEventsByTypeAsync(long lastVersion);
    }
}