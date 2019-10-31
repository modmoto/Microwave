using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class EventRepositoryInMemory : IEventRepository
    {
        private readonly BlockingCollection<DomainEventWrapper> _domainEvents = new BlockingCollection<DomainEventWrapper>();
        private object _lock = new object();
        public long CurrentCache { get; private set; }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long lastVersion = 0)
        {
            if (entityId == null) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(null));
            var mongoCollection = _domainEvents;
            var domainEventDbos = mongoCollection.Where(e => e.DomainEvent.EntityId == entityId && e.Version > lastVersion).ToList();
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
                    GlobalVersion = dbo.GlobalVersion,
                    Version = dbo.Version,
                    DomainEvent = dbo.DomainEvent
                };
            });

            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents));
        }

        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            lock (_lock)
            {
                var maxVersion = _domainEvents.
                                     Where(e => e.DomainEvent.EntityId == domainEvents.First().EntityId)
                                     .OrderBy(e => e.Version).LastOrDefault()?.Version ?? 0;
                if (maxVersion != currentEntityVersion) return Task.FromResult(
                    Result.ConcurrencyResult(currentEntityVersion, maxVersion));
                var newVersion = currentEntityVersion;

                var domainEventWrappers = new List<DomainEventWrapper>();
                foreach (var domainEvent in domainEvents)
                {
                    CurrentCache++;
                    var domainEventWrapper = new DomainEventWrapper
                    {
                        GlobalVersion = CurrentCache,
                        DomainEvent = domainEvent,
                        Version = ++newVersion
                    };
                    domainEventWrappers.Add(domainEventWrapper);
                }

                foreach (var eventWrapper in domainEventWrappers)
                {
                    _domainEvents.Add(eventWrapper);
                }
                return Task.FromResult(Result.Ok());
            }
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long lastVersion = 0)
        {
            var domainEventWrappers = _domainEvents.OrderBy(e => e.GlobalVersion).Where(e => e.GlobalVersion > lastVersion);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long
        lastVersion = 0)
        {
            var domainEventWrappers = _domainEvents
                .OrderBy(e => e.GlobalVersion)
                .Where(e => e.DomainEventType == eventType && e.GlobalVersion > lastVersion);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }
    }
}