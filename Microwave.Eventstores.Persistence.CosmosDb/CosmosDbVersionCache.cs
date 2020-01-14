using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Persistence.MongoDb.Eventstores;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbVersionCache : IVersionCache
    {
        private readonly ICosmosDb _cosmosDb;
        private DocumentClient _client;
        private readonly ConcurrentDictionary<Identity, long> _cache = new ConcurrentDictionary<Identity, long>();

        public CosmosDbVersionCache(ICosmosDb cosmosDb)
        {
            _cosmosDb = cosmosDb;
            _client = cosmosDb.GetCosmosDbClient();
        }

        public async Task<long> Get(Identity entityId)
        {
            if (!_cache.TryGetValue(entityId, out var version))
            {
                var actualVersion = await GetVersionFromDb(entityId);
                _cache[entityId] = actualVersion;
                return actualVersion;
            }

            return version;
        }

        private async Task<long> GetVersionFromDb(Identity entityId)
        {
            var events = _client.CreateDocumentQuery<DomainEventWrapper>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.EventsCollectionId),
                    new FeedOptions {MaxItemCount = -1})
                    .Where(e => e.DomainEvent.EntityId.Id == entityId.Id);
            var actualVersion = events.Max(e => e.Version);
            return actualVersion;
        }

        public async Task<long> GetForce(Identity entityId)
        {
            var actualVersion = await GetVersionFromDb(entityId);
            _cache[entityId] = actualVersion;
            return actualVersion;
        }

        public void Update(Identity entityId, long actualVersion)
        {
            _cache[entityId] = actualVersion;
        }
    }
}
