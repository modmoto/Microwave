using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
   public class CosmosDbSnapshotRepository : ISnapShotRepository
    {
        private readonly ICosmosDb _cosmosDb;
        private DocumentClient _client;

        public CosmosDbSnapshotRepository(ICosmosDb cosmosDb)
        {
            _cosmosDb = cosmosDb;
            _client = _cosmosDb.GetCosmosDbClient();
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new()
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
                var entity = JsonConvert.DeserializeObject<T>(wrappedEvent.GetValue("Entity").ToString());
                var version = (long)wrappedEvent.GetValue(nameof(DomainEventWrapper.Version));
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

        public async Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.SnapshotsCollectionId), snapShot);
        }
    }
}
