using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public class VersionCache : IVersionCache
    {
        private readonly ConcurrentDictionary<string, long> _cache = new ConcurrentDictionary<string, long>();
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";
        private object _lock = new object();

        public VersionCache(MicrowaveMongoDb mongoDb)
        {
            _database = mongoDb.Database;
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

        public long GlobalVersion { get; private set; }
        public void CountUpGlobalVersion()
        {
            lock (_lock)
            {
                GlobalVersion++;
            }
        }
    }
}