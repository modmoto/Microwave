using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Discovery.Subscriptions
{
    public interface ISubscriptionRepository
    {
        Task StoreSubscription(Subscription subscription);
        Task<IEnumerable<Subscription>> LoadSubscriptions();
        Task<DateTimeOffset> GetCurrentVersion(Subscription subscription);
    }
}