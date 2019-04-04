using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.Persistence.MongoDb.EventStores
{
    public interface IVersionCache
    {
        Task<long> Get(Identity entityId);
        Task<long> GetForce(Identity entityId);
        void Update(Identity entityId, long actualVersion);
    }
}