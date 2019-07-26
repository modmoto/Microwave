using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public interface ICosmosDb
    {
        string DatabaseId { get; }

        string EventsCollectionId { get; }
        string SnapshotsCollectionId { get; }
        DocumentClient GetCosmosDbClient();
    }
}