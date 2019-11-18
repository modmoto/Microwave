using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public class EventRepositoryMongoDb : IEventRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";
        private readonly IVersionCache _versions;
        private object _lock = new object();

        public EventRepositoryMongoDb(MicrowaveMongoDb mongoDb, IVersionCache versions)
        {
            _versions = versions;
            _database = mongoDb.Database;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long lastEntityStreamVersion = 0)
        {
            if (entityId == null) return Result<IEnumerable<DomainEventWrapper>>.NotFound(null);
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = await mongoCollection
                .Find(ev => ev.Key.EntityId == entityId && ev.Key.EntityStreamVersion > lastEntityStreamVersion)
                .SortBy(s => s.OverallVersion)
                .ToListAsync();
            if (!domainEventDbos.Any())
            {
                var eventDbos = await mongoCollection.Find(ev => ev.Key.EntityId == entityId).FirstOrDefaultAsync();
                if (eventDbos == null) return Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId);
                return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());
            }

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    OverallVersion = dbo.OverallVersion,
                    EntityStreamVersion = dbo.Key.EntityStreamVersion,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long lastOverallVersion = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = await mongoCollection
                .Find(ev => ev.OverallVersion > lastOverallVersion)
                .SortBy(s => s.OverallVersion)
                .ToListAsync();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    OverallVersion = dbo.OverallVersion,
                    EntityStreamVersion = dbo.Key.EntityStreamVersion,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(
            string eventType,
            long lastOverallVersion = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventTypeDbos = await mongoCollection
                .Find(ev => ev.EventType == eventType && ev.OverallVersion > lastOverallVersion)
                .SortBy(s => s.OverallVersion)
                .ToListAsync();

            var domainEvents = domainEventTypeDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    OverallVersion = dbo.OverallVersion,
                    EntityStreamVersion = dbo.Key.EntityStreamVersion,
                    DomainEvent = dbo.Payload
                };
            });
            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            lock (_lock)
            {
                var events = domainEvents.ToList();
                if (!events.Any()) return Result.Ok();

                var entityId = events.First().EntityId;
                var versionTemp = currentEntityVersion;
                var lastVersion = _versions.Get(entityId).Result;

                if (lastVersion < currentEntityVersion) return Result.ConcurrencyResult(currentEntityVersion, lastVersion);

                var domainEventDbos = events.Select(domainEvent =>
                {
                    _versions.CountUpGlobalVersion();
                    return new DomainEventDbo
                    {
                        Payload = domainEvent,
                        OverallVersion = _versions.OverallVersion,
                        Key = new DomainEventKey
                        {
                            EntityStreamVersion = ++versionTemp,
                            EntityId = domainEvent.EntityId
                        },
                        EventType = domainEvent.GetType().Name
                    };
                }).ToList();

                try
                {
                    _database.GetCollection<DomainEventDbo>(_eventCollectionName).InsertManyAsync(domainEventDbos).Wait();
                    _versions.Update(entityId, versionTemp);
                }
                catch (AggregateException aggregateException)
                {
                    var innerException = aggregateException.InnerExceptions.SingleOrDefault();
                    if (innerException?.GetType() == typeof(MongoBulkWriteException<DomainEventDbo>))
                    {
                        var actualVersion = _versions.GetForce(entityId).Result;
                        return Result.ConcurrencyResult(currentEntityVersion, actualVersion);
                    }

                    throw;
                }
                return Result.Ok();
            }
        }
    }
}