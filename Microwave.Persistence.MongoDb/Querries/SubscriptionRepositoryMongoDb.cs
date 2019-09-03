using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Discovery.Subscriptions;
using Microwave.Queries.Ports;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class SubscriptionRepositoryMongoDb : ISubscriptionRepository
    {
        private readonly IVersionRepository _versionRepository;
        private readonly IMongoDatabase _dataBase;
        private string _subscriptionCollection = "ServiceSubscriptions";

        public SubscriptionRepositoryMongoDb(MicrowaveMongoDb eventMongoDb, IVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
            _dataBase = eventMongoDb.Database;
        }

        public async Task StoreSubscriptionAsync(Subscription subscription)
        {
            var mongoCollection = _dataBase.GetCollection<SubscriptionDto>(_subscriptionCollection);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<SubscriptionDto>();
            findOneAndReplaceOptions.IsUpsert = true;

            var subDto = new SubscriptionDto {
                SubscribedEvent = subscription.SubscribedEvent,
                SubscriberUrl = subscription.SubscriberUrl
            };

            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<SubscriptionDto, bool>>) (e =>
                    e.Id == subDto.Id),
                subDto, findOneAndReplaceOptions);
        }

        public async Task<IEnumerable<Subscription>> LoadSubscriptionsAsync()
        {
            var mongoCollection = _dataBase.GetCollection<SubscriptionDto>(_subscriptionCollection);
            var subscriptions = (await mongoCollection.FindAsync(version => true)).ToList();
            return subscriptions.Select(s => new Subscription(s.SubscribedEvent, s.SubscriberUrl));
        }

        public Task<DateTimeOffset> GetCurrentVersion(Subscription subscription)
        {
            return _versionRepository.GetVersionAsync(subscription.SubscribedEvent);
        }
    }

    public class SubscriptionDto
    {
        public string Id => $"{SubscriberUrl}-{SubscribedEvent}";
        public string SubscribedEvent { get; set; }
        public Uri SubscriberUrl { get; set; }
    }
}