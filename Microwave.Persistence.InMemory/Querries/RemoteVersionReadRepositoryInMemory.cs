using System;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class RemoteVersionReadRepositoryInMemory : IRemoteVersionReadRepository
    {
        private readonly SharedMemoryClass _sharedMemoryClass;

        public RemoteVersionReadRepositoryInMemory(SharedMemoryClass sharedMemoryClass)
        {
            _sharedMemoryClass = sharedMemoryClass;
        }

        public Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            return _sharedMemoryClass.Get(domainEventType);
        }
    }
}