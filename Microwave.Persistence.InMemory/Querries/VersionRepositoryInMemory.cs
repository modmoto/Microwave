using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class VersionRepositoryInMemory : IVersionRepository
    {
        private Dictionary<string, DateTimeOffset> versionDictionray = new Dictionary<string, DateTimeOffset>();
        public Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            if (domainEventType == null) return Task.FromResult(DateTimeOffset.MinValue);
            if (!versionDictionray.TryGetValue(domainEventType, out var version)) return Task.FromResult(DateTimeOffset.MinValue);

            return Task.FromResult(version);
        }

        public Task SaveVersion(LastProcessedVersion version)
        {
            versionDictionray[version.EventType] = version.LastVersion;
            return Task.CompletedTask;
        }
    }
}