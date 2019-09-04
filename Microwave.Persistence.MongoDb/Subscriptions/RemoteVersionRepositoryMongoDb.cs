using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Subscriptions
{
    public class RemoteVersionRepositoryMongoDb : IRemoteVersionRepository
    {
        private readonly IVersionRepository _versionRepository;
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedVersions = "LastProcessedRemoteVersions";

        public RemoteVersionRepositoryMongoDb(IVersionRepository versionRepository, MicrowaveMongoDb eventMongoDb)
        {
            _versionRepository = versionRepository;
            _dataBase = eventMongoDb.Database;
        }

        public Task<DateTimeOffset> GetCurrentVersion(Subscription subscription)
        {
            return _versionRepository.GetVersionAsync(subscription.SubscribedEvent);
        }

        public async Task SaveRemoteVersionAsync(RemoteVersion version)
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

        public Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion)
        {
            throw new NotImplementedException();
        }
    }
}