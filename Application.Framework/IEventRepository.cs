using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventRepository
    {
        Task<IEnumerable<DomainEvent>> LoadEvents(Guid entityId);
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
    }
}