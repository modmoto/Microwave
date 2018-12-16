using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class VersionRepository : IVersionRepository
    {
        private readonly IMongoDatabase _dataBase;

        public VersionRepository(IMongoDatabase dataBase)
        {
            _dataBase = dataBase;
        }

        public async Task<long> GetVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>("LastProcessedVersions");
            var lastProcessedVersion = (await mongoCollection.FindAsync(version => version.EventType == domainEventType)).FirstOrDefault();
            if (lastProcessedVersion == null) return 0L;
            return lastProcessedVersion.LastVersion;
        }

        public async Task SaveVersion(LastProcessedVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>("LastProcessedVersions");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<LastProcessedVersionDbo>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(IsSameType(version),
                new LastProcessedVersionDbo
            {
                EventType = version.EventType,
                LastVersion = version.LastVersion
            }, findOneAndReplaceOptions);
        }

        private static Expression<Func<LastProcessedVersionDbo, bool>> IsSameType(LastProcessedVersion version)
        {
            return e => e.EventType == version.EventType;
        }
    }
}