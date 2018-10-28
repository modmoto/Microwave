using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;

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
            var stream =
                await _eventStoreReadContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEventTypeName);
            if (stream == null) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());
            var domainEvents = stream.DomainEvents.Where(ev => ev.Version > from)
                .Select(dbo =>
                {
                    var domainEvent = _objectConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    domainEvent.DomainEventId = dbo.Id;
                    return domainEvent;
                });
            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result> AppendToTypeStream(DomainEvent domainEvent)
        {
            var typeStream =
                await _eventStoreReadContext.TypeStreams
                    .Include(ev => ev.DomainEvents)
                    .FirstOrDefaultAsync(str => str.DomainEventType == domainEvent.GetType().Name);

            if (typeStream == null)
            {
                typeStream = new TypeStream
                {
                    DomainEvents = new List<DomainEventDboCopy>(),
                    DomainEventType = domainEvent.GetType().Name,
                    Version = -1
                };

                _eventStoreReadContext.TypeStreams.Add(typeStream);
            }

            var entityVersionTemp = typeStream.Version + 1;

            var domainEventDbo = CreateDomainEventCopy(domainEvent);

            domainEventDbo.Version = entityVersionTemp;

            typeStream.Version = entityVersionTemp;
            typeStream.DomainEvents.Add(domainEventDbo);

            await _eventStoreReadContext.SaveChangesAsync();
            return Result.Ok();
        }

        private DomainEventDboCopy CreateDomainEventCopy(DomainEvent domainEventWrapper)
        {
            var payLoad = _objectConverter.Serialize(domainEventWrapper);
            return new DomainEventDboCopy
            {
                Payload = payLoad,
                Created = domainEventWrapper.Created,
                Id = domainEventWrapper.DomainEventId,
                Version = domainEventWrapper.Version
            };
        }

    }
}