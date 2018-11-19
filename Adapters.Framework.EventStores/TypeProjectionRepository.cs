using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class TypeProjectionRepository : ITypeProjectionRepository
    {
        private readonly IObjectConverter _objectConverter;
        private readonly EventStoreReadContext _eventStoreReadContext;

        public TypeProjectionRepository(IObjectConverter objectConverter, EventStoreReadContext eventStoreReadContext)
        {
            _objectConverter = objectConverter;
            _eventStoreReadContext = eventStoreReadContext;
        }

    public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string domainEventTypeName,
            long from = -1)
        {
            var stream = _eventStoreReadContext.TypeStreams
                .Where(str => str.DomainEventType == domainEventTypeName && str.Version > from).ToList();

            if (!stream.Any()) return Result<IEnumerable<DomainEventWrapper>>.Ok(new List<DomainEventWrapper>());

            var domainEvents = stream.Select(dbo =>
            {
                return new DomainEventWrapper
                {
                    Created = dbo.Created,
                    Version = dbo.Version,
                    DomainEvent = _objectConverter.Deserialize<IDomainEvent>(dbo.Payload)
                };
            });
            return Result<IEnumerable<DomainEventWrapper>>.Ok(domainEvents);
        }

        public async Task<Result> AppendToTypeStream(IDomainEvent domainEvent)
        {
            return await AppendToStreamWithName(domainEvent.GetType().Name, domainEvent);
        }

        public async Task<Result> AppendToStreamWithName(string streamName, IDomainEvent domainEvent)
        {
            var typeStream = _eventStoreReadContext.TypeStreams
                .Where(str => str.DomainEventType == streamName).ToList();

            var entityVersionTemp = typeStream.Count;

            var domainEventDbo = new DomainEventTypeDbo
            {
                Payload = _objectConverter.Serialize(domainEvent),
                DomainEventType = streamName,
                Created = DateTime.UtcNow.Ticks,
                Version = entityVersionTemp
            };

            _eventStoreReadContext.TypeStreams.Add(domainEventDbo);

            await _eventStoreReadContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}