using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class SnapShotRepositoryInMemory : ISnapShotRepository
    {
        private readonly Dictionary<Identity, object> _snapShots = new Dictionary<Identity, object>();

        public Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new()
        {
            if (entityId == null) return Task.FromResult(SnapShotResult<T>.NotFound(null));
            if (!_snapShots.TryGetValue(entityId, out var entity)) return Task.FromResult(SnapShotResult<T>
            .Default());
            var snapShotWrapper = entity as SnapShotWrapper<T>;
            return Task.FromResult(new SnapShotResult<T>(snapShotWrapper.Entity, snapShotWrapper.Version));
        }

        public Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            _snapShots[snapShot.Id] = snapShot;
            return Task.CompletedTask;
        }
    }
}