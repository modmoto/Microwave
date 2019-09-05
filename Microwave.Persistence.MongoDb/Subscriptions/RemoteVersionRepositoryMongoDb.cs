using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Subscriptions
{
    public class RemoteVersionRepositoryMongoDb : IRemoteVersionRepository
    {
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedRemoteVersions = "LastProcessedRemoteVersions";
        private readonly string _lastProcessedVersions = "LastProcessedVersions";

        public RemoteVersionRepositoryMongoDb(MicrowaveMongoDb eventMongoDb)
        {
            _dataBase = eventMongoDb.Database;
        }

        public async Task<SubscriptionState> GetSubscriptionState(Subscription subscription)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == subscription.SubscribedEvent)).FirstOrDefault();
            var ret = lastProcessedVersion?.LastVersion ?? DateTimeOffset.MinValue;
            return new SubscriptionState(ret, default(DateTimeOffset));
        }

        public async Task SaveRemoteVersionAsync(RemoteVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedRemoteVersions);

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