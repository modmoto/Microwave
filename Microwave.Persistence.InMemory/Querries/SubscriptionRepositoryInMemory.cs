using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Discovery.Subscriptions;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class SubscriptionRepositoryInMemory : ISubscriptionRepository
    {
        private readonly IVersionRepository _versionRepository;

        public SubscriptionRepositoryInMemory(IVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
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