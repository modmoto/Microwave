using System;
using System.Collections.Concurrent;
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
        private readonly BlockingCollection<DomainEventWrapper> _domainEvents = new BlockingCollection<DomainEventWrapper>();

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0)
        {
            if (entityId == null) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(null));
            var mongoCollection = _domainEvents;
            var domainEventDbos = mongoCollection.Where(e => e.DomainEvent.EntityId == entityId && e.Version > from).ToList();
            if (!domainEventDbos.Any())
            {
                var eventDbos = mongoCollection.FirstOrDefault(e => e.DomainEvent.EntityId == entityId);
                if (eventDbos == null) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId));
                return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>()));
            }

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Version,
                    DomainEvent = dbo.DomainEvent
                };
            });

            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents));
        }

        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            var maxVersion = _domainEvents.
                                 Where(e => e.DomainEvent.EntityId == domainEvents.First().EntityId)
                                 .OrderBy(e => e.Version).LastOrDefault()?.Version ?? 0;
            if (maxVersion != currentEntityVersion) return Task.FromResult(
                Result.ConcurrencyResult(currentEntityVersion, maxVersion));
            var newVersion = currentEntityVersion;
            var domainEventWrappers = domainEvents.Select(e => new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                DomainEvent = e,
                Version = ++newVersion
            }).ToArray();
            foreach (var eventWrapper in domainEventWrappers)
            {
                _domainEvents.Add(eventWrapper);
            }
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
    }
}