using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbEventRepository : IEventRepository
    {
        private readonly ICosmosDbClient _cosmosDbClient;
        private readonly ICosmosDb _cosmosDb;
        private DocumentClient _client;
        private IEnumerable<Type> _domainEventTypes;


        public CosmosDbEventRepository(ICosmosDbClient cosmosDbClient, ICosmosDb cosmosDb, IEnumerable<Assembly> assemblies)
        {
            _cosmosDbClient = cosmosDbClient;
            _cosmosDb = cosmosDb;
            _client = cosmosDb.GetCosmosDbClient();
            var type = typeof(IDomainEvent);
            _domainEventTypes = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long @from = 0)
        {
            var result = await _cosmosDbClient.GetDomainEventsAsync(entityId, from);
            return Result<IEnumerable<DomainEventWrapper>>.Ok(result);
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
            var result = await _cosmosDbClient.LoadEventsByTypeAsync(eventType, tickSince);
            return Result<IEnumerable<DomainEventWrapper>>.Ok(result);
        }

        public async Task<Result<DateTimeOffset>> GetLastEventOccuredOn(string domainEventType)
        {
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.DomainEventType == domainEventType)
                .AsDocumentQuery();

            var wrappedEvents = new List<JObject>();
            while (query.HasMoreResults)
            {
                wrappedEvents.AddRange(await query.ExecuteNextAsync<JObject>());
            }

            var result = new List<DomainEventWrapper>();
            foreach (var wrappedEvent in wrappedEvents)
            {
                result.Add(new DomainEventWrapper
                {
                    Created = (DateTimeOffset)wrappedEvent.GetValue("Created"),
                    DomainEvent = (IDomainEvent)JsonConvert.DeserializeObject(wrappedEvent.GetValue("DomainEvent").ToString(), _domainEventTypes.Single(x => x.Name == wrappedEvent.GetValue("DomainEventType").ToString())),
                    Version = (long)wrappedEvent.GetValue("Version")
                });
            }

            return Result<DateTimeOffset>.Ok(result.Max(s => s.Created));
        }
    }
}
