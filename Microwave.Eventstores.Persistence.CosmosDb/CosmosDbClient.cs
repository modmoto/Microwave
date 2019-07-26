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
using Microwave.EventStores.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbClient : ICosmosDbClient
    {
        private readonly ICosmosDb _cosmosDb;
        private readonly DocumentClient _client;
        private IEnumerable<Type> _domainEventTypes;
       

        public CosmosDbClient(ICosmosDb cosmosDb, IEnumerable<Assembly> assemblies)
        {
            _cosmosDb = cosmosDb;
            _client = cosmosDb.GetCosmosDbClient();
            var type = typeof(IDomainEvent);
            _domainEventTypes = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

        }

        public async Task<IEnumerable<DomainEventWrapper>> GetDomainEventsAsync(Identity identity)
        {   
            var query = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.DomainEvent.EntityId  == identity)
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

            return result;
        }

        public async Task<SnapShotResult<T>> LoadSnapshotAsync<T>(Identity entityId)
        {
            var query = _client.CreateDocumentQuery<SnapShotWrapper<T>>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.SnapshotsCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.Id == entityId)
                .AsDocumentQuery();

            var wrappedEvents = new List<JObject>();
            while (query.HasMoreResults)
            {
                wrappedEvents.AddRange(await query.ExecuteNextAsync<JObject>());
            }
            var allSnapshots = new List<SnapShotWrapper<T>>();
            foreach (var wrappedEvent in wrappedEvents)
            {
                var entity =  JsonConvert.DeserializeObject<T>(wrappedEvent.GetValue("Entity").ToString());
                var version = (long) wrappedEvent.GetValue("Version");
                Guid.TryParse(wrappedEvent.GetValue("id").ToString(), out var guid);
                Identity identity = null;
                if (guid != Guid.Empty)
                {
                    identity = Identity.Create(guid);
                }
                else
                {
                    identity = Identity.Create(wrappedEvent.GetValue("id").ToString());
                }
                allSnapshots.Add(new SnapShotWrapper<T>(entity, identity, version));
            }
            var result = allSnapshots.Single(x => x.Version == allSnapshots.Max(s => s.Version));
            return new SnapShotResult<T>(result.Entity, result.Version);
        }

        public async Task SaveSnapshotAsync<T>(SnapShotWrapper<T> snapShot)
        {
            await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.SnapshotsCollectionId), snapShot);
        }


        public async Task<IEnumerable<DomainEventWrapper>> GetDomainEventsAsync(DateTimeOffset tickSince)
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
                    Created = (DateTimeOffset) wrappedEvent.GetValue("Created"),
                    DomainEvent = (IDomainEvent)JsonConvert.DeserializeObject(wrappedEvent.GetValue("DomainEvent").ToString(), _domainEventTypes.Single(x => x.Name == wrappedEvent.GetValue("DomainEventType").ToString())),
                    Version = (long) wrappedEvent.GetValue("Version")
                });
            }

            return result;
        }

        public async Task<Result> CreateItemAsync(DomainEventWrapper domainEvent)
        {
            try
            {
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId), domainEvent);
            }
            catch (DocumentClientException e)
            {
                var actualVersion = (await GetDomainEventsAsync(domainEvent.DomainEvent.EntityId)).Max(x => x.Version);
                return Result.ConcurrencyResult(domainEvent.Version, actualVersion);
            }
            return Result.Ok();
        }

        public async Task<IEnumerable<DomainEventWrapper>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince)
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

            return result;
        }
    }

}