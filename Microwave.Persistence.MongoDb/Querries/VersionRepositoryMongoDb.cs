using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Queries.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class VersionRepositoryMongoDb : IVersionRepository
    {
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedVersions = "LastProcessedVersions";
        private readonly string _lastProcessedRemoteVersions = "LastProcessedRemoteVersions";

        public VersionRepositoryMongoDb(MicrowaveMongoDb dataBase)
        {
            _dataBase = dataBase.Database;
        }

        public async Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            return await GetVersion(domainEventType, mongoCollection);
        }

        public async Task<DateTimeOffset> GetRemoteVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedRemoteVersions);
            return await GetVersion(domainEventType, mongoCollection);
        }

        public async Task SaveVersionAsync(LastProcessedVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);

            await SaveVersion(version, mongoCollection);
        }

        public async Task SaveRemoteVersionAsync(LastProcessedVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedRemoteVersions);

            await SaveVersion(version, mongoCollection);
        }

        private static async Task SaveVersion(LastProcessedVersion version, IMongoCollection<LastProcessedVersionDbo> mongoCollection)
        {
            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<LastProcessedVersionDbo>();
            findOneAndReplaceOptions.IsUpsert = true;

            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<LastProcessedVersionDbo, bool>>) (e => e.EventType == version.EventType),
                new LastProcessedVersionDbo
                {
                    EventType = version.EventType,
                    LastVersion = version.LastVersion
                }, findOneAndReplaceOptions);
        }

        private static async Task<DateTimeOffset> GetVersion(string domainEventType, IMongoCollection<LastProcessedVersionDbo> mongoCollection)
        {
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == domainEventType)).FirstOrDefault();
            if (lastProcessedVersion == null) return DateTimeOffset.MinValue;
            return lastProcessedVersion.LastVersion;
        }
    }
}