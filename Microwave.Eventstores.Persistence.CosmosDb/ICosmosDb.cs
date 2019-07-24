using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public interface ICosmosDb
    {
        DocumentClient GetCosmosDbClient();
    }
}