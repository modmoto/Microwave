using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;

namespace Microwave.Persistence.InMemory.Subscriptions
{
    public class SubscriptionRepositoryInMemory : ISubscriptionRepository
    {
        private readonly BlockingCollection<Subscription> _subscriptions = new BlockingCollection<Subscription>();

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
    }
}