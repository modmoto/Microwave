using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbEventRepository : IEventRepository
    {
        private readonly ICosmosDb _cosmosDb;
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly IVersionCache _versionCache;
        private DocumentClient _client;
        private IEnumerable<Type> _domainEventTypes;


        public CosmosDbEventRepository(ICosmosDb cosmosDb, IAssemblyProvider assemblyProvider, IVersionCache versionCache)
        {
            _cosmosDb = cosmosDb;
            _assemblyProvider = assemblyProvider;
            _versionCache = versionCache;
            _client = cosmosDb.GetCosmosDbClient();
            var type = typeof(IDomainEvent);
            _domainEventTypes = assemblyProvider.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByEntity(Identity entityId, long @from = 0)
        {
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.DomainEvent.EntityId == entityId && e.Version >= from)
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
                    Created = (DateTimeOffset)wrappedEvent.GetValue(nameof(DomainEventWrapper.Created)),
                    DomainEvent = (IDomainEvent)JsonConvert.DeserializeObject(wrappedEvent.GetValue(nameof(DomainEventWrapper.DomainEvent)).ToString(), _domainEventTypes.Single(x => x.Name == wrappedEvent.GetValue(nameof(DomainEventWrapper.DomainEvent)).ToString())),
                    Version = (long)wrappedEvent.GetValue(nameof(DomainEventWrapper.Version))
                });
            }

            return Result<IEnumerable<DomainEventWrapper>>.Ok(result);
        }

        public async Task<Result> AppendAsync(IEnumerable<IDomainEvent> domainEvents, long currentEntityVersion)
        {
            foreach (var domainEvent in domainEvents)
            {
               
                var wrappedEvent = new DomainEventWrapper
                {
                    DomainEvent = domainEvent,
                    Created = DateTimeOffset.Now,
                    Version = currentEntityVersion
                };
                try
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId), wrappedEvent);
                }
                catch (DocumentClientException)
                {
                    var actualVersion = await _versionCache.Get(domainEvent.EntityId);
                    return Result.ConcurrencyResult(currentEntityVersion, actualVersion);
                }
               
            }
            return Result.Ok();
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEvents(DateTimeOffset tickSince = default(DateTimeOffset))
        {
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.Created >= tickSince)
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
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.Created >= tickSince && e.DomainEventType == eventType)
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
