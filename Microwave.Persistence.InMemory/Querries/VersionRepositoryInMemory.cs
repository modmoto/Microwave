using System;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class VersionRepositoryInMemory : IVersionRepository
    {
        public Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            throw new NotImplementedException();
        }

        public Task SaveVersion(LastProcessedVersion version)
        {
            throw new NotImplementedException();
        }
    }
}