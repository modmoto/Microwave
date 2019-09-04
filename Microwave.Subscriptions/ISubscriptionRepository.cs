using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Subscriptions
{
    public interface ISubscriptionRepository
    {
        Task StoreSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Subscription>> LoadSubscriptionsAsync();
        Task<DateTimeOffset> GetCurrentVersion(Subscription subscription);
        Task SaveRemoteVersionAsync(RemoteVersion version);
        Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion);
    }
}