using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace Microwave.Eventstores.Persistence.CosmosDb
{
    public interface ICosmosDb
    {
        DocumentClient GetCosmosDbClient();
    }
}