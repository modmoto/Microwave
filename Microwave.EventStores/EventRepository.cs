using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventRepository : IEventRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";
        private readonly VersionCache _versions;

        public EventRepository(EventDatabase database, VersionCache versions)
        {
            _versions = versions;
            _database = database.Database;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Key.EntityId == entityId.Id && ev.Key.Version > from)).ToList();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId.ToString());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long tickSince = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Created > tickSince)).ToList();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long version)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventTypeDbos = (await mongoCollection.FindAsync(ev => ev.EventType == eventType && ev.Created > version)).ToList();

            if (!domainEventTypeDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventTypeDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = dbo.Payload
                };
            });
            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            var events = domainEvents.ToList();
            if (!events.Any()) return Result.Ok();

            var entityId = events.First().EntityId;
            var versionTemp = currentEntityVersion;
            var lastVersion = await LastVersion(entityId);
            _versions[entityId] = versionTemp;

            if (lastVersion < currentEntityVersion) return Result.ConcurrencyResult(currentEntityVersion, lastVersion);

            var domainEventDbos = events.Select(domainEvent =>
            {
                return new DomainEventDbo
                {
                    Payload = domainEvent,
                    Created = DateTime.UtcNow.Ticks,
                    Key = new DomainEventKey
                    {
                        Version = ++versionTemp,
                        EntityId = domainEvent.EntityId.Id
                    },
                    EventType = domainEvent.GetType().Name
                };
            }).ToList();

            try
            {
                await _database.GetCollection<DomainEventDbo>(_eventCollectionName).InsertManyAsync(domainEventDbos);
                _versions[entityId] = versionTemp;
            }
            catch (MongoBulkWriteException)
            {
                var actualVersion = await GetVersionFromDb(entityId);
                _versions[entityId] = actualVersion;
                return Result.ConcurrencyResult(currentEntityVersion, actualVersion);
            }
            return Result.Ok();
        }

        private async Task<long> GetVersionFromDb(Identity entityId)
        {
            var cursorReloaded = await _database.GetCollection<DomainEventDbo>(_eventCollectionName)
                .FindAsync(v => v.Key.EntityId == entityId.Id);
            var eventDbosReloaded = await cursorReloaded.ToListAsync();
            var actualVersion = eventDbosReloaded.LastOrDefault()?.Key.Version ?? 0;
            return actualVersion;
        }

        private async Task<long> LastVersion(Identity entityId)
        {
            if (!_versions.TryGetValue(entityId, out var version))
            {
                var actualVersion = await GetVersionFromDb(entityId);
                return actualVersion;
            }

            return version;
        }
    }

    public class VersionCache : ConcurrentDictionary<Identity, long>
    {
    }
}