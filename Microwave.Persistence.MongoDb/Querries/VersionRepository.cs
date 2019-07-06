using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Queries;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class VersionRepository : IVersionRepository
    {
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedVersions = "LastProcessedVersions";

        public VersionRepository(MicrowaveMongoDb dataBase)
        {
            _dataBase = dataBase.Database;
        }

        public async Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            var lastProcessedVersion = (await mongoCollection.FindAsync(version => version.EventType == domainEventType)).FirstOrDefault();
            if (lastProcessedVersion == null) return DateTimeOffset.MinValue;
            return lastProcessedVersion.LastVersion;
        }

        public async Task SaveVersion(LastProcessedVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);

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
    }
}