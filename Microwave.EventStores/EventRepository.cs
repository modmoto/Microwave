using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.ObjectPersistences;
using Microwave.Queries;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventRepository : IEventRepository
    {
        private readonly DomainEventDeserializer _domainEventConverter;
        private readonly IMongoDatabase _database;
        private readonly IObjectConverter _converter;

        public EventRepository(
            IMongoDatabase database,
            DomainEventDeserializer domainEventConverter,
            IObjectConverter converter)
        {
            _domainEventConverter = domainEventConverter;
            _database = database;
            _converter = converter;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Guid entityId, long from = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>("DomainEventDbos");
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Key.EntityId == entityId.ToString() && ev.Key.Version > from)).ToList();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.NotFound(entityId.ToString());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = _domainEventConverter.Deserialize(dbo.Payload)
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(long tickSince = 0)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>("DomainEventDbos");
            var domainEventDbos = (await mongoCollection.FindAsync(ev => ev.Created > tickSince)).ToList();
            if (!domainEventDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventDbos.Select(dbo =>
            {
                var domainEvent = _domainEventConverter.Deserialize(dbo.Payload);
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = domainEvent
                };
            });

            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, long version)
        {
            var mongoCollection = _database.GetCollection<DomainEventDbo>("DomainEventDbos");
            var domainEventTypeDbos = (await mongoCollection.FindAsync(ev => ev.EventType == eventType && ev.Created > version)).ToList();

            if (!domainEventTypeDbos.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = domainEventTypeDbos.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Key.Version,
                    DomainEvent = _domainEventConverter.Deserialize(dbo.Payload)
                };
            });
            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            if (!events.Any()) return Result.Ok();

            var entityVersionTemp = entityVersion;

            var mongoCollection = _database.GetCollection<DomainEventDbo>("DomainEventDbos");
            var entityId = events.First().EntityId.ToString();

            var cursor = await mongoCollection.FindAsync(v => v.Key.EntityId == entityId);
            var eventDbos = await cursor.ToListAsync();
            var lastVersion = eventDbos.LastOrDefault()?.Key.Version ?? 0;
            if (lastVersion < entityVersion) return Result.ConcurrencyResult(entityVersion, lastVersion);

            var domainEventDbos = events.Select(domainEvent =>
            {
                return new DomainEventDbo
                {
                    Payload = _converter.Serialize(domainEvent),
                    Created = DateTime.UtcNow.Ticks,
                    Key = new DomainEventKey
                    {
                        Version = ++entityVersionTemp,
                        EntityId = domainEvent.EntityId.ToString()
                    },
                    EventType = domainEvent.GetType().Name
                };
            }).ToList();

            try
            {
                await mongoCollection.InsertManyAsync(domainEventDbos);
            }
            catch (MongoBulkWriteException)
            {
                return Result.ConcurrencyResult(entityVersion, eventDbos.Last()?.Key.Version ?? 0);
            }
            return Result.Ok();
        }
    }
}