using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class OverallProjectionRepository : IOverallProjectionRepository
    {
        private readonly IObjectConverter _objectConverter;
        private readonly EventStoreReadContext _eventStoreReadContext;

        public OverallProjectionRepository(IObjectConverter objectConverter, EventStoreReadContext eventStoreReadContext)
        {
            _objectConverter = objectConverter;
            _eventStoreReadContext = eventStoreReadContext;
        }

        public async Task<Result> AppendToOverallStream(IEnumerable<DomainEvent> events)
        {
            var allStream = _eventStoreReadContext.OverallStream;

            var entityVersionTemp = allStream.Count();
            foreach (var domainEvent in events)
            {
                var domainEventDbo = CreateDomainEventCopyOverall(domainEvent);
                entityVersionTemp++;
                domainEventDbo.Version = entityVersionTemp;

                allStream.Add(domainEventDbo);
            }

            await _eventStoreReadContext.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadOverallStream(long from = -1)
        {
            var domainEventDboCopyOverallStreams = _eventStoreReadContext.OverallStream.Where(ev => ev.Version > from).ToList();
            var loadOverallStream = domainEventDboCopyOverallStreams.Select(dbo =>
            {
                var domainEvent = _objectConverter.Deserialize<DomainEvent>(dbo.Payload);
                domainEvent.Version = dbo.Version;
                domainEvent.Created = dbo.Created;
                domainEvent.DomainEventId = new Guid(dbo.Id);
                return domainEvent;
            });
            return Result<IEnumerable<DomainEvent>>.Ok(loadOverallStream);
        }

        private DomainEventDboCopyOverallStream CreateDomainEventCopyOverall(DomainEvent domainEventWrapper)
        {
            var payLoad = _objectConverter.Serialize(domainEventWrapper);
            return new DomainEventDboCopyOverallStream
            {
                Payload = payLoad,
                Created = domainEventWrapper.Created,
                Id = domainEventWrapper.DomainEventId.ToString(),
                Version = domainEventWrapper.Version
            };
        }
    }
}