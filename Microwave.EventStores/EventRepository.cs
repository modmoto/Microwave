using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventRepository : IEventRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";
        private readonly IVersionCache _versions;

        public EventRepository(EventDatabase database, IVersionCache versions)
        {
            _versions = versions;
            _database = database.Database;
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

            if (!domainEventTypeDbos.Any())
            {
                var eventDbos = await mongoCollection.Find(ev => ev.EventType == eventType).FirstOrDefaultAsync();
                if (eventDbos == null) return Result<IEnumerable<DomainEventWrapper>>.NotFound(StringIdentity.Create(eventType));
                return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());
            }

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