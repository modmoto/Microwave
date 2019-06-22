using System.Threading.Tasks;
using Microwave.Domain.Identities;

namespace Microwave.Eventstores.Persistence.MongoDb
{
    public interface IVersionCache
    {
        Task<long> Get(Identity entityId);
        Task<long> GetForce(Identity entityId);
        void Update(Identity entityId, long actualVersion);
    }
}