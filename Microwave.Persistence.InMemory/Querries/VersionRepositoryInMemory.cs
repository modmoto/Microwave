using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class VersionRepositoryInMemory : IVersionRepository
    {
        private readonly ConcurrentDictionary<string, long> _versionDictionray =
            new ConcurrentDictionary<string, long>();
        public Task<long> GetVersionAsync(string domainEventType)
        {
            if (domainEventType == null) return Task.FromResult(0L);
            if (!_versionDictionray.TryGetValue(domainEventType, out var version)) return Task.FromResult(0L);

            return Task.FromResult(version);
        }

        public Task SaveVersion(LastProcessedVersion version)
        {
            _versionDictionray[version.EventType] = version.LastVersion;
            return Task.CompletedTask;
        }
    }
}