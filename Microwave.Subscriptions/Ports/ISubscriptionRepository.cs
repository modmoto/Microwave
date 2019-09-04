using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Subscriptions.Ports
{
    public interface ISubscriptionRepository
    {
        Task StoreSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Subscription>> LoadSubscriptionsAsync();
    }
}