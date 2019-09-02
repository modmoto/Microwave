using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Discovery.Subscriptions;
using Microwave.Queries.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class SubscriptionRepositoryMongoDb : ISubscriptionRepository
    {
        private readonly IVersionRepository _versionRepository;
        private readonly IMongoDatabase _dataBase;

        public SubscriptionRepositoryMongoDb(MicrowaveMongoDb eventMongoDb, IVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
            _dataBase = eventMongoDb.Database;
        }

        public Task StoreSubscription(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Subscription>> LoadSubscriptions()
        {
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset> GetCurrentVersion(Subscription subscription)
        {
            return _versionRepository.GetVersionAsync(subscription.SubscribedEvent);
        }
    }
}