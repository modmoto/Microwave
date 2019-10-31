using System.Threading.Tasks;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public interface IVersionCache
    {
        Task<long> Get(string entityId);
        Task<long> GetForce(string entityId);
        void Update(string entityId, long actualVersion);
        long OverallVersion { get; }
        void CountUpGlobalVersion();
    }
}