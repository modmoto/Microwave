using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Application.Ports
{
    public interface IEntityStreamRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Guid entityId, long from = 0);
        Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long tickSince = 0);
    }

    public interface IOverallProjectionRepository
    {
        Task<Result> AppendToOverallStream(IEnumerable<IDomainEvent> resultValue);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadOverallStream(long from = 0);
    }

    public interface ITypeProjectionRepository
    {
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string domainEventTypeName, long from = 0);
        Task<Result> AppendToTypeStream(IDomainEvent domainEvent);
        Task<Result> AppendToStreamWithName(string streamName, IDomainEvent domainEvent);
    }
}