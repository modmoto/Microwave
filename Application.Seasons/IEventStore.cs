using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Seasons
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainResultDomainEvents);
        Task<T> LoadAsync<T>(Guid commandEntityId);
    }
}