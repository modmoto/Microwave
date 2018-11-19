using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework.Results;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEntityStreamRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Guid entityId, long from = -1);
        Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsSince(long tickSince);
    }

    public interface IOverallProjectionRepository
    {
        Task<Result> AppendToOverallStream(IEnumerable<DomainEvent> resultValue);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadOverallStream(long from = -1);
    }

    public interface ITypeProjectionRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string domainEventTypeName, long from = -1);
        Task<Result> AppendToTypeStream(DomainEvent domainEvent);
        Task<Result> AppendToStreamWithName(string streamName, DomainEvent domainEvent);
    }

    public class DomainEventWrapper
    {
        public long Created { get; set; }
        public long Version { get; set; }
        public DomainEvent DomainEvent { get; set; }
    }
}