using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents);
        T LoadAsync<T>(Guid commandEntityId) where T : Entity,  new();
    }
}