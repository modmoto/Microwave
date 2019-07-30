using System.Threading.Tasks;
using Microwave.Domain.Identities;

namespace Microwave.EventStores.Ports
{
    internal interface ISnapShotRepository
    {
        Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new();
        Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot);
    }

    internal class SnapShotWrapper<T>
    {
        public SnapShotWrapper(T entity, Identity id, long version)
        {
            Entity = entity;
            Version = version;
            Id = id;
        }

        public T Entity { get; }
        public long Version { get; }
        public Identity Id { get; }
    }

    internal class SnapShotResult<T> where T : new()
    {
        public static SnapShotResult<T> Default()
        {
            return new SnapShotResult<T>(new T(), 0);
        }

        public SnapShotResult(T entity, long version)
        {
            Version = version;
            Entity = entity;
        }

        public long Version { get; }
        public T Entity { get; }
    }
}