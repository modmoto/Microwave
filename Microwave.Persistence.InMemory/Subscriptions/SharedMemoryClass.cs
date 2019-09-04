using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microwave.Subscriptions;

namespace Microwave.Persistence.InMemory.Subscriptions
{
    public class SharedMemoryClass
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _versionDictionary = new ConcurrentDictionary<string, DateTimeOffset>();

        public Task<DateTimeOffset> Get(string domainEventType)
        {
            if (domainEventType == null) return Task.FromResult(DateTimeOffset.MinValue);
            var value = !_versionDictionary.TryGetValue(domainEventType, out var version)
                ? DateTimeOffset.MinValue
                : version;
            return Task.FromResult(value);
        }

        public void Save(RemoteVersion version)
        {
            _versionDictionary[version.EventType] = version.LastVersion;
        }
    }
}