using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IVersionCache
    {
        Task<long> Get(Identity entityId);
        Task<long> GetForce(Identity entityId);
        void Update(Identity entityId, long actualVersion);
    }
}