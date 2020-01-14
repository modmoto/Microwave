using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbStatusRepository : IStatusRepository
    {
        private readonly ICosmosDb _cosmosDb;
        private DocumentClient _client;

        public CosmosDbStatusRepository(ICosmosDb cosmosDb)
        {
            _cosmosDb = cosmosDb;
            _client = cosmosDb.GetCosmosDbClient();
        }

        public async Task SaveEventLocation(EventLocation eventLocation)
        {
            var previousEventLocation = _client.CreateDocumentQuery<Document>(
                UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId),
                new FeedOptions {MaxItemCount = -1}).AsEnumerable().SingleOrDefault();

            if (previousEventLocation == null)
            {
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId), eventLocation);
            }
            else
            {
                await _client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId,
                        previousEventLocation.ResourceId), eventLocation);
            }
        }

        public async Task<EventLocation> GetEventLocation()
        {
            var eventLocation = _client.CreateDocumentQuery<EventLocation>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId),
                    new FeedOptions { MaxItemCount = -1 }).AsEnumerable().FirstOrDefault();

            return eventLocation;
        }

        public async Task<ServiceMap> GetServiceMap()
        {
            var serviceMap = _client.CreateDocumentQuery<ServiceMap>(
                UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId),
                new FeedOptions { MaxItemCount = -1 }).AsEnumerable().FirstOrDefault();

            return serviceMap;
        }

        public async Task SaveServiceMap(ServiceMap map)
        {
            var previousEventLocation = _client.CreateDocumentQuery<Document>(
                UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId),
                new FeedOptions { MaxItemCount = -1 }).AsEnumerable().SingleOrDefault();

            if (previousEventLocation == null)
            {
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId), map);
            }
            else
            {
                await _client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(_cosmosDb.DatabaseId, _cosmosDb.StatusCollectionId,
                        previousEventLocation.ResourceId), map);
            }
        }
    }
}