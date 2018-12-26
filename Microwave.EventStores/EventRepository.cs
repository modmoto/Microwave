using System;
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

        public EventRepository(EventDatabase database)
        {
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

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currenEntityVersion)
        {
            var events = domainEvents.ToList();
            if (!events.Any()) return Result.Ok();

            var entityId = events.First().EntityId.Id;
            var versionTemp = currenEntityVersion;
            var lastVersion = await GetLastVersion(entityId);
            if (lastVersion < currenEntityVersion) return Result.ConcurrencyResult(currenEntityVersion, lastVersion);

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

            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            try
            {
                await mongoCollection.InsertManyAsync(domainEventDbos);
            }
            catch (MongoBulkWriteException)
            {
                var cursorReloaded = await mongoCollection.FindAsync(v => v.Key.EntityId == entityId);
                var eventDbosReloaded = await cursorReloaded.ToListAsync();
                return Result.ConcurrencyResult(currenEntityVersion, eventDbosReloaded.LastOrDefault()?.Key.Version ?? 0);
            }
            return Result.Ok();
        }

        private async Task<long> GetLastVersion(string entityId)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var cursor = await mongoCollection.FindAsync(v => v.Key.EntityId == entityId);
            var eventDbos = await cursor.ToListAsync();
            var lastVersion = eventDbos.LastOrDefault()?.Key.Version ?? 0;
            return lastVersion;
        }
    }
}