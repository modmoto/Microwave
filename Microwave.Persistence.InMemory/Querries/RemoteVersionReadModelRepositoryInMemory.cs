using System;
using System.Threading.Tasks;
using Microwave.Persistence.InMemory.Subscriptions;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class RemoteVersionReadModelRepositoryInMemory : IRemoteVersionReadModelRepository
    {
        private readonly SharedMemoryClass _sharedMemoryClass;

        public RemoteVersionReadModelRepositoryInMemory(SharedMemoryClass sharedMemoryClass)
        {
            _sharedMemoryClass = sharedMemoryClass;
        }

        public Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            return _sharedMemoryClass.Get(domainEventType);
        }
    }
}