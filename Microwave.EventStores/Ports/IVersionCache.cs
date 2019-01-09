using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.EventStores.Ports
{
    public interface IVersionCache
    {
        Task<long> Get(string entityId);
        Task<long> GetForce(string entityId);
        void Update(string entityId, long actualVersion);
    }
}