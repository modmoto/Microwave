using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStoreFacade
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<T> LoadAsync<T>(Guid entityId) where T : Entity, new ();
    }
}