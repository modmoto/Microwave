using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;

namespace Microwave.EventStores.Ports
{
    public interface ISnapShotRepository
    {
        Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new();
        Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot);
    }

    public class SnapShotWrapper<T>
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

    public class SnapShotResult<T> : Result<T> where T : new()
    {
        public static SnapShotResult<T> Default()
        {
            return new SnapShotResult<T>(new T(), 0);
        }

        public new static SnapShotResult<T> NotFound(Identity identity)
        {
            return new SnapShotResult<T>(new T(), 0, new NotFound(typeof(T), identity));
        }

        public SnapShotResult(T value, long version) : base(new Ok())
        {
            Version = version;
            Value = value;
        }

        private SnapShotResult(T value, long version, NotFound notFound) : base(notFound)
        {
            Version = version;
            Value = value;
        }

        public long Version { get; }
    }
}