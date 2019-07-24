using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbClient : ICosmosDbClient
    {
        private readonly DocumentClient _client;
        private IEnumerable<Type> _domainEventTypes;
        private const string DatabaseId = "Eventstore";
        private const string CollectionId = "DomainEvents";

        public CosmosDbClient(ICosmosDb cosmosDb, IEnumerable<Assembly> assemblies)
        {
            _client = cosmosDb.GetCosmosDbClient();
            var type = typeof(IDomainEvent);
            _domainEventTypes = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

        }

        public async Task InitializeCosmosDbAsync()
        {
            var database = await  _client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId });
            var collection = await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseId),
                new DocumentCollection { Id = CollectionId });
            if (database == null || collection == null)
            {
                throw new ArgumentException("Could not create Database or Collection with given CosmosDb Configuration Parameters!");
            }
        }


        public async Task CreateDomainEventAsync(IDomainEvent domainEvent)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
            await _client.CreateDocumentAsync(uri, domainEvent);
           
        }


        public async Task<IEnumerable<IDomainEvent>> GetDomainEventsAsync(Identity identity)
        {   
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.DomainEvent.EntityId  == identity)
                .AsDocumentQuery();

            var wrappedEvents = new List<JObject>();
            while (query.HasMoreResults)
            {
                wrappedEvents.AddRange(await query.ExecuteNextAsync<JObject>());
            }

            var result = wrappedEvents.Select(e =>  JsonConvert.DeserializeObject(e.GetValue("DomainEvent").ToString(), _domainEventTypes.Single(x => x.Name == e.GetValue("DomainEventType").ToString()))).ToList();
            return new List<IDomainEvent>();
        }


        public async Task<Result<IEnumerable<DomainEventWrapper>>> GetDomainEventsAsync(DateTimeOffset tickSince)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            var uri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(uri, queryOptions)
                .Where(e => e.Created > tickSince);
            return Result<IEnumerable<DomainEventWrapper>>.Ok(query.ToList());
        }

        public async Task<Document> CreateItemAsync(DomainEventWrapper domainEvent)
        {
            return await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), domainEvent);
        }

        public async Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            var uri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(uri, queryOptions)
                .Where(e => e.DomainEventType == eventType);
            return Result<IEnumerable<DomainEventWrapper>>.Ok(query.ToList());
        }
    }

}