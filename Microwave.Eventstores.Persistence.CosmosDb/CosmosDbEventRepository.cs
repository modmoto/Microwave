using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbEventRepository : IEventRepository
    {
        private readonly ICosmosDbClient _cosmosDbClient;


        public CosmosDbEventRepository(ICosmosDbClient cosmosDbClient)
        {
            _cosmosDbClient = cosmosDbClient;
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long @from = 0)
        {
            throw new NotImplementedException();
            //var uri = CreateUriForCosmosDb(entityId);
            //var domainEvents = (await _client.ReadDocumentAsync<List<DomainEventWrapper>>(uri)).Document;
            //return new EventStoreResult<IEnumerable<DomainEventWrapper>>(domainEvents, domainEvents.Max(e => e.Version));
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            foreach (var domainEvent in domainEvents)
            {
               
                await _cosmosDbClient.CreateItemAsync(new DomainEventWrapper
                {
                    DomainEvent = domainEvent,
                    Created = DateTimeOffset.Now,
                    Version = currentEntityVersion
                });
            }

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset))
        {
            var result = await _cosmosDbClient.GetDomainEventsAsync(tickSince);
            if (result.Any())
            {
                return Result<IEnumerable<DomainEventWrapper>>.Ok(result);
            }
            else
            {
                return Result<IEnumerable<DomainEventWrapper>>.NotFound(null);
            }
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince = default(DateTimeOffset))
        {
            var result = _cosmosDbClient.LoadEventsByTypeAsync(eventType, tickSince);
            return Result<IEnumerable<DomainEventWrapper>>.Ok(result.Result);
        }

        public async Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType)
        {
            throw new NotImplementedException();
        }
    }
}
