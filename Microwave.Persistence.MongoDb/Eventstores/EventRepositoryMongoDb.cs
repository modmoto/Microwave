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

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(string entityId, long lastVersion = 0)
        {
            if (entityId == null) return Result<IEnumerable<DomainEventWrapper>>.NotFound(null);
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Key.EntityId == entityId && ev.Key.Version > lastVersion)).ToList();
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
                    GlobalVersion = dbo.GlobalVersion,
                    Version = dbo.Key.Version,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long lastVersion = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.GlobalVersion > lastVersion)).ToList();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    GlobalVersion = dbo.GlobalVersion,
                    Version = dbo.Key.Version,
                    DomainEvent = dbo.Payload
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(
            string eventType,
            long lastVersion = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>(_eventCollectionName);
            var domainEventTypeDbos = (await mongoCollection.FindAsync(ev => ev.EventType == eventType && ev.GlobalVersion > lastVersion)).ToList();

            var domainEvents = domainEventTypeDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    GlobalVersion = dbo.GlobalVersion,
                    Version = dbo.Key.Version,
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
                        GlobalVersion = _versions.GlobalVersion,
                        Key = new DomainEventKey
                        {
                            Version = ++versionTemp,
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