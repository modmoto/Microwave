using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public class EventRepository : IEventRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";
        private readonly IVersionCache _versions;

        public EventRepository(MicrowaveMongoDb mongoDb, IVersionCache versions)
        {
            _versions = versions;
            _database = mongoDb.Database;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long from = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Key.EntityId == entityId.Id && ev.Key.Version > from)).ToList();
            if (!domainEventDbos.Any())
            {
                var eventDbos = await mongoCollection.Find(ev => ev.Key.EntityId == entityId.Id).FirstOrDefaultAsync();
                if (eventDbos == null) return Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId);
                return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());
            }

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

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset))
        {
            if (tickSince == default(DateTimeOffset)) tickSince = DateTimeOffset.MinValue;
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

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince = default(DateTimeOffset))
        {
            if (tickSince == default(DateTimeOffset)) tickSince = DateTimeOffset.MinValue;
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventTypeDbos = (await mongoCollection.FindAsync(ev => ev.EventType == eventType && ev.Created > tickSince)).ToList();

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

        public async Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var dbo = await mongoCollection
                .Find(e => e.EventType == domainEventType)
                .SortByDescending(a => a.Created)
                .FirstOrDefaultAsync();
            return dbo == null ? Result<DateTimeOffset>.NotFound(StringIdentity.Create(domainEventType)) : Result<DateTimeOffset>.Ok(dbo.Created);
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            var events = domainEvents.ToList();
            if (!events.Any()) return Result.Ok();

            var entityId = events.First().EntityId;
            var versionTemp = currentEntityVersion;
            var lastVersion = await _versions.Get(entityId);

            if (lastVersion < currentEntityVersion) return Result.ConcurrencyResult(currentEntityVersion, lastVersion);

            var domainEventDbos = events.Select(domainEvent =>
            {
                return new DomainEventDbo
                {
                    Payload = domainEvent,
                    Created = DateTimeOffset.Now,
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
                _versions.Update(entityId, versionTemp);
            }
            catch (MongoBulkWriteException)
            {
                var actualVersion = await _versions.GetForce(entityId);
                return Result.ConcurrencyResult(currentEntityVersion, actualVersion);
            }
            return Result.Ok();
        }
    }
}