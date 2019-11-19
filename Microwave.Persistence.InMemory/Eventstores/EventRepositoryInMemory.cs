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
        public EventRepositoryInMemory(IEnumerable<IDomainEvent> domainEvents = null)
        {
            var events = domainEvents?.ToList() ?? new List<IDomainEvent>();
            var groupedEvents = events.GroupBy(de => de.EntityId).ToList();

            foreach (var evs in groupedEvents)
            {
                var result = Append(evs, 0);
                result.Check();
            }
        }

        private readonly ConcurrentDictionary<string, IEnumerable<DomainEventWrapper>> _domainEvents =
            new ConcurrentDictionary<string, IEnumerable<DomainEventWrapper>>();
        private readonly object _lock = new object();
        private long _currentCache;

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long lastEntityStreamVersion = 0)
        {
            if (entityId == null) return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(null));
            if (!_domainEvents.TryGetValue(entityId, out var eventsOfEntity))
                return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId));
            var domainEventDbos = eventsOfEntity
                .Where(e => e.EntityStreamVersion > lastEntityStreamVersion)
                .OrderBy(s => s.OverallVersion)
                .ToList();
            if (!domainEventDbos.Any())
            {
                return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>()));
            }

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    OverallVersion = dbo.OverallVersion,
                    EntityStreamVersion = dbo.EntityStreamVersion,
                    DomainEvent = dbo.DomainEvent
                };
            });

            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents));

        }

        public Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            lock (_lock)
            {
                return Task.FromResult(Append(domainEvents, currentEntityVersion));
            }
        }

        private Result Append(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            var eventsToAdd = domainEvents.ToList();
            var entityId = eventsToAdd.First().EntityId;
            if (!_domainEvents.ContainsKey(entityId))
            {
                _domainEvents[entityId] = new List<DomainEventWrapper>();
            }

            var eventWrappers = _domainEvents[entityId].ToList();
            var maxVersion = eventWrappers.OrderBy(e => e.OverallVersion)
                                 .LastOrDefault()?
                                 .EntityStreamVersion ?? 0;
            if (maxVersion != currentEntityVersion) return Result.ConcurrencyResult(currentEntityVersion, maxVersion);
            var newVersion = currentEntityVersion;

            var domainEventWrappers = new List<DomainEventWrapper>();
            domainEventWrappers.AddRange(eventWrappers);
            foreach (var domainEvent in eventsToAdd)
            {
                _currentCache++;
                var domainEventWrapper = new DomainEventWrapper
                {
                    OverallVersion = _currentCache,
                    DomainEvent = domainEvent,
                    EntityStreamVersion = ++newVersion
                };
                domainEventWrappers.Add(domainEventWrapper);
            }

            _domainEvents[entityId] = domainEventWrappers;
            return Result.Ok();
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long lastOverallVersion = 0)
        {
            var domainEvents = _domainEvents.SelectMany(e => e.Value);
            var domainEventWrappers = domainEvents
                .Where(e => e.OverallVersion > lastOverallVersion)
                .OrderBy(e => e.OverallVersion);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }

        public Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long
        lastOverallVersion = 0)
        {
            var domainEvents = _domainEvents.SelectMany(e => e.Value);
            var domainEventWrappers = domainEvents
                .Where(e => e.DomainEventType == eventType && e.OverallVersion > lastOverallVersion)
                .OrderBy(e => e.OverallVersion);
            return Task.FromResult(Result<IEnumerable<DomainEventWrapper>>.Ok(domainEventWrappers));
        }
    }
}