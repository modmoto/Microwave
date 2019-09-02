using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Discovery.Subscriptions
{
    public interface ISubscriptionRepository
    {
        Task StoreSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Subscription>> LoadSubscriptionsAsync();
        Task<DateTimeOffset> GetCurrentVersion(Subscription subscription);
    }
}