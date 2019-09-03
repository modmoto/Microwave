using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Discovery.Subscriptions
{
    public interface ISubscriptionHandler
    {
        Task SubscribeOnDiscoveredServices();
        Task StoreSubscription(Subscription subscription);
        Task PushNewChanges();
        Task<IEnumerable<Subscription>> GetSubscriptions();
    }
}