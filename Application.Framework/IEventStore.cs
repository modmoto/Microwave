using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents);
        Task<T> LoadAsync<T>(Guid commandEntityId) where T : Entity,  new();
        Task<IEnumerable<DomainEvent>> GetEvents();
    }
}