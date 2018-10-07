using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework.Results;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStoreFacade
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId) where T : new();
    }
}