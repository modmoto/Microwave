using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class VersionCache : IVersionCache
    {
        private readonly ConcurrentDictionary<string, long> _cache = new ConcurrentDictionary<string, long>();
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";

        public VersionCache(EventDatabase database)
        {
            _database = database.Database;
        }

        public async Task<long> Get(string entityId)
        {
            if (!_cache.TryGetValue(entityId, out var version))
            {
                var actualVersion = await GetVersionFromDb(entityId);
                _cache[entityId] = actualVersion;
                return actualVersion;
            }

            return version;
        }

        private async Task<long> GetVersionFromDb(string entityId)
        {
            var cursorReloaded = await _database.GetCollection<DomainEventDbo>(_eventCollectionName)
                .FindAsync(v => v.Key.EntityId == entityId);
            var eventDbosReloaded = await cursorReloaded.ToListAsync();
            var actualVersion = eventDbosReloaded.LastOrDefault()?.Key.Version ?? 0;
            return actualVersion;
        }

        public async Task<long> GetForce(string entityId)
        {
            var actualVersion = await GetVersionFromDb(entityId);
            _cache[entityId] = actualVersion;
            return actualVersion;
        }

        public void Update(string entityId, long actualVersion)
        {
            _cache[entityId] = actualVersion;
        }
    }
}