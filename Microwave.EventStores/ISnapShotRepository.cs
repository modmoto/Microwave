using System;
using System.Threading.Tasks;

namespace Microwave.EventStores
{
    public interface ISnapShotRepository
    {
        Task<SnapShotResult<T>> LoadSnapShot<T>(Guid entityId) where T : new();
        Task SaveSnapShot<T>(T snapShot, Guid entityId, long version);
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