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
        private readonly string _lastProcessedVersionsOld = "LastProcessedVersionsHistory";

        public RemoteVersionRepositoryMongoDb(MicrowaveMongoDb eventMongoDb)
        {
            _dataBase = eventMongoDb.Database;
        }

        public async Task<SubscriptionState> GetSubscriptionState(Subscription subscription)
        {
            var currentVersion = await GetCurrentVersionOfEventType(subscription);
            var oldVersion = await GetLastVersionOfEventType(subscription);

            return new SubscriptionState(currentVersion, oldVersion);
        }

        public async Task<DateTimeOffset> GetCurrentVersionOfEventType(Subscription subscription)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            var eventType = subscription.SubscribedEvent;
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == eventType)).FirstOrDefault();
            var currentVersion = lastProcessedVersion?.LastVersion ?? DateTimeOffset.MinValue;
            return currentVersion;
        }

        public async Task SaveCurrentVersionAsLastVersion(string eventType, DateTimeOffset newVersion)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedRemoteVersions);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<LastProcessedVersionDbo> { IsUpsert = true };

            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<LastProcessedVersionDbo, bool>>) (e => e.EventType == eventType),
                new LastProcessedVersionDbo
                {
                    EventType = eventType,
                    LastVersion = newVersion
                }, findOneAndReplaceOptions);
        }

        public async Task<DateTimeOffset> GetLastVersionOfEventType(Subscription subscription)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersionsOld);
            var eventType = subscription.SubscribedEvent;
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == eventType)).FirstOrDefault();
            var currentVersion = lastProcessedVersion?.LastVersion ?? DateTimeOffset.MinValue;
            return currentVersion;
        }

        public async Task SaveRemoteVersionAsync(RemoteVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedRemoteVersions);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<LastProcessedVersionDbo> { IsUpsert = true };

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