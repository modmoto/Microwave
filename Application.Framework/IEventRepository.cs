using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework.Results;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventRepository
    {
        Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = -1);
        Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<Result<IEnumerable<DomainEvent>>> LoadEventsByTypeAsync(string domainEventTypeName, long from = -1);
    }
}