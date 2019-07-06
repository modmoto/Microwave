using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public class VersionCache : IVersionCache
    {
        private readonly ConcurrentDictionary<Identity, long> _cache = new ConcurrentDictionary<Identity, long>();
        private readonly IMongoDatabase _database;
        private readonly string _eventCollectionName = "DomainEventDbos";

        public VersionCache(MicrowaveMongoDb mongoDb)
        {
            _database = mongoDb.Database;
        }

        public async Task<long> Get(Identity entityId)
        {
            if (!_cache.TryGetValue(entityId, out var version))
            {
                var actualVersion = await GetVersionFromDb(entityId);
                _cache[entityId] = actualVersion;
                return actualVersion;
            }

            return version;
        }

        private async Task<long> GetVersionFromDb(Identity entityId)
        {
            var cursorReloaded = await _database.GetCollection<DomainEventDbo>(_eventCollectionName)
                .FindAsync(v => v.Key.EntityId == entityId.Id);
            var eventDbosReloaded = await cursorReloaded.ToListAsync();
            var actualVersion = eventDbosReloaded.LastOrDefault()?.Key.Version ?? 0;
            return actualVersion;
        }

        public async Task<long> GetForce(Identity entityId)
        {
            var actualVersion = await GetVersionFromDb(entityId);
            _cache[entityId] = actualVersion;
            return actualVersion;
        }

        public void Update(Identity entityId, long actualVersion)
        {
            _cache[entityId] = actualVersion;
        }
    }
}