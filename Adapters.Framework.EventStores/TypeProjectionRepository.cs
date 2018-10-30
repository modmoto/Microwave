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

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByTypeAsync(string domainEventTypeName,
            long from = -1)
        {
            var stream = _eventStoreReadContext.TypeStreams
                .Where(str => str.DomainEventType == domainEventTypeName && str.Version > from).ToList();

            if (!stream.Any()) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());

            var domainEvents = stream.Select(dbo =>
                {
                    var domainEvent = _objectConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result> AppendToTypeStream(DomainEvent domainEvent)
        {
            return await AppendToStreamWithName(domainEvent.GetType().Name, domainEvent);
        }

        public async Task<Result> AppendToStreamWithName(string streamName, DomainEvent domainEvent)
        {
            var typeStream = _eventStoreReadContext.TypeStreams
                .Where(str => str.DomainEventType == streamName).ToList();

            var entityVersionTemp = typeStream.Count;

            var domainEventDbo = CreateDomainEventCopy(streamName, domainEvent);

            domainEventDbo.Version = entityVersionTemp;
            _eventStoreReadContext.TypeStreams.Add(domainEventDbo);

            await _eventStoreReadContext.SaveChangesAsync();
            return Result.Ok();
        }

        private DomainEventTypeDbo CreateDomainEventCopy(string streamName, DomainEvent domainEventWrapper)
        {
            var payLoad = _objectConverter.Serialize(domainEventWrapper);
            return new DomainEventTypeDbo
            {
                Payload = payLoad,
                DomainEventType = streamName,
                Created = domainEventWrapper.Created,
                Version = domainEventWrapper.Version
            };
        }

    }
}