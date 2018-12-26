using System.Threading.Tasks;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public interface ISnapShotRepository
    {
        Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new();
        Task SaveSnapShot<T>(T snapShot, Identity entityId, long version);
    }

    public class SnapShotResult<T>
    {
        public SnapShotResult(T entity, long version)
        {
            Version = version;
            Entity = entity;
        }

        public long Version { get; }
        public T Entity { get; }
    }
}