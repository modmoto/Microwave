using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class SnapShotRepositoryInMemory : ISnapShotRepository
    {
        private readonly ConcurrentDictionary<string, object> _snapShots = new ConcurrentDictionary<string, object>();

        public Task<SnapShotResult<T>> LoadSnapShot<T>(string entityId) where T : new()
        {
            if (entityId == null) return Task.FromResult(SnapShotResult<T>.NotFound(null));
            if (!_snapShots.TryGetValue(entityId, out var entity))
                return Task.FromResult(SnapShotResult<T>.Default());
            var snapShotWrapper = (SnapShotWrapper<T>) entity;
            return Task.FromResult(new SnapShotResult<T>(snapShotWrapper.Entity, snapShotWrapper.Version));
        }

        public Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            _snapShots[snapShot.Id] = snapShot;
            return Task.CompletedTask;
        }
    }
}