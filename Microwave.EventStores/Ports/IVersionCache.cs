using System.Threading.Tasks;

namespace Microwave.EventStores.Ports
{
    public interface IVersionCache
    {
        Task<long> Get(string entityId);
        Task<long> GetForce(string entityId);
        void Update(string entityId, long actualVersion);
    }
}