using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.EventStores;
using Microwave.Persistence.MongoDb;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries.Ports;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbVersionRepository : IVersionRepository
    {
        private readonly ICosmosDb _cosmosDb;
        private readonly string _lastProcessedVersions = "LastProcessedVersions";
        private DocumentClient _client;

        public CosmosDbVersionRepository(ICosmosDb cosmosDb)
        {
            _cosmosDb = cosmosDb;
            _client = cosmosDb.GetCosmosDbClient();
        }

        public async Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            var query = _client.CreateDocumentQuery<LastProcessedVersionDbo>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.VersionCollectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.EventType == domainEventType)
                .AsDocumentQuery();
            var versions = new List<LastProcessedVersionDbo>();
            while (query.HasMoreResults)
            {
                versions.AddRange(await query.ExecuteNextAsync<LastProcessedVersionDbo>());
            }

            var lastProcessedVersion = versions.Where(version => version.EventType == domainEventType).FirstOrDefault();
            if (lastProcessedVersion == null) return DateTimeOffset.MinValue;
            return lastProcessedVersion.LastVersion;
        }

        public async Task SaveVersion(LastProcessedVersion version)
        {
            await _client.UpsertDocumentAsync(_cosmosDb.VersionCollectionId, version);
        }
    }
}