using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDb : ICosmosDb
    {
        public SecureString PrimaryKey { get; set; }

        public Uri CosmosDbLocation { get; set; }

        public DocumentClient GetCosmosDbClient()
        {
            var client = new DocumentClient(CosmosDbLocation, PrimaryKey);
            return client;
        }

        public string DatabaseId => "Eventstore";
        public string EventsCollectionId => "DomainEvents";
        public string SnapshotsCollectionId => "Snapshots";
        public string ServiceMapCollectionId => "ServiceMap";
        public string StatusCollectionId => "Status";

        public async Task InitializeCosmosDb()
        {
            var client = new DocumentClient(CosmosDbLocation, PrimaryKey);

            var domainEventsCollection = new DocumentCollection
            {
                Id = EventsCollectionId
            };
            domainEventsCollection.UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys =
                    new Collection<UniqueKey>
                    {
                        new UniqueKey {Paths = new Collection<string> {"/Version", "/DomainEvent/EntityId/Id"}}
                    }
            };

            var snapShotCollection = new DocumentCollection
            {
                Id = SnapshotsCollectionId
            };
            snapShotCollection.UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys =
                    new Collection<UniqueKey>
                    {
                        new UniqueKey {Paths = new Collection<string> {"/Version", "/Id/Id"}}
                    }
            };

            try
            {
                await client.CreateDatabaseIfNotExistsAsync(new Database {Id = DatabaseId});
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId),
                    domainEventsCollection);
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId),
                    new DocumentCollection{Id = StatusCollectionId});
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId),
                    snapShotCollection);
            }
            catch (DocumentClientException e)
            {
                throw new ArgumentException(
                    $"Could not create Database or Collection with given CosmosDb Configuration Parameters! Exception : {e}" );

            }
        }
    }
}