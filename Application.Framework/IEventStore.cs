using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents);
        Task<T> LoadAsync<T>(Guid commandEntityId) where T : new();
        Task<IEnumerable<DomainEvent>> GetEvents(Guid entityId, int from = 0, int to = 100);
    }
}