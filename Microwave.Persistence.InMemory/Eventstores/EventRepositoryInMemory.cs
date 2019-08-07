using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class EventRepositoryInMemory : IEventRepository
    {
        private readonly List<DomainEventWrapper> _domainEvents = new List<DomainEventWrapper>();

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0)
        {
            var domainEventWrappersById = _domainEvents.Where(e => e.DomainEvent.EntityId == entityId);
            var domainEventWrappers = domainEventWrappersById.Where(e => e.Version > from).OrderBy(e => e.Version);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }

        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            var maxVersion = _domainEvents.
                                 Where(e => e.DomainEvent.EntityId == domainEvents.First().EntityId)
                                 .OrderBy(e => e.Version).FirstOrDefault()?.Version ?? 0;
            if (maxVersion > currentEntityVersion) return Task.FromResult(
                Result.ConcurrencyResult(currentEntityVersion, maxVersion));
            var newVersion = currentEntityVersion;
            var domainEventWrappers = domainEvents.Select(e => new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                DomainEvent = e,
                Version = ++newVersion
            });
            _domainEvents.AddRange(domainEventWrappers);
            return Task.FromResult(Result.Ok());
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset))
        {
            var domainEventWrappers = _domainEvents.OrderBy(e => e.Created).Where(e => e.Created > tickSince);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset
        tickSince = default(DateTimeOffset))
        {
            var domainEventWrappers = _domainEvents
                .OrderBy(e => e.Created)
                .Where(e => e.DomainEventType == eventType && e.Created > tickSince);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }

        public Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType)
        {
            return null;
        }
    }
}