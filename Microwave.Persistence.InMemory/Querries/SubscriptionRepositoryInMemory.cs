using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Discovery.Subscriptions;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class SubscriptionRepositoryInMemory : ISubscriptionRepository
    {
        private readonly IVersionRepository _versionRepository;
        private readonly SharedMemoryClass _sharedMemoryClass;
        private readonly BlockingCollection<Subscription> _subscriptions = new BlockingCollection<Subscription>();

        public SubscriptionRepositoryInMemory(
            IVersionRepository versionRepository,
            SharedMemoryClass sharedMemoryClass)
        {
            _versionRepository = versionRepository;
            _sharedMemoryClass = sharedMemoryClass;
        }

        public Task StoreSubscriptionAsync(Subscription subscription)
        {
            if (!_subscriptions.Contains(subscription))
            {
                _subscriptions.Add(subscription);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Subscription>> LoadSubscriptionsAsync()
        {
            var subscriptions = (IEnumerable<Subscription>) _subscriptions.ToList();
            return Task.FromResult(subscriptions);
        }

        public Task<DateTimeOffset> GetCurrentVersion(Subscription subscription)
        {
            return _versionRepository.GetVersionAsync(subscription.SubscribedEvent);
        }

        public Task SaveVersionAsync(RemoteVersion version)
        {
            _sharedMemoryClass.Save(version);
            return Task.CompletedTask;

        }
    }
}