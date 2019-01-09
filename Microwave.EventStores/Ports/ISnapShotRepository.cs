using System.Threading.Tasks;

namespace Microwave.EventStores.Ports
{
    public interface ISnapShotRepository
    {
        Task<SnapShotResult<T>> LoadSnapShot<T>(string entityId) where T : new();
        Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot);
    }

    public class SnapShotWrapper<T>
    {
        public SnapShotWrapper(T entity, string id, long version)
        {
            Entity = entity;
            Version = version;
            Id = id;
        }

        public T Entity { get; }
        public long Version { get; }
        public string Id { get; }
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