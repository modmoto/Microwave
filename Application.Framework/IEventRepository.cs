using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventRepository
    {
        Task<IEnumerable<DomainEvent>> LoadEventsByEntity(Guid entityId, long from = 0);
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<IEnumerable<T>> LoadEventsByTypeAsync<T>(long from = 0) where T : DomainEvent;
        Task<IEnumerable<DomainEvent>> LoadEventsByTypeAsync(string domainEventTypeName, long from = 0);
    }
}