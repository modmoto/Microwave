using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IPublishedEventStream
    {
        Task<IEnumerable<T>> GetEventsByTypeAsync<T>(long lastVersion) where T : DomainEvent;
    }
}