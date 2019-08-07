using System;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.InMemory.Eventstores
{
    public class SnapShotRepositoryInMemory : ISnapShotRepository
    {
        public Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            throw new NotImplementedException();
        }
    }
}