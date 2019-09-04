using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Subscriptions
{
    public interface ISubscriptionHandler
    {
        Task SubscribeOnDiscoveredServices();
        Task StoreSubscription(Subscription subscription);
        Task PushNewChanges();
        Task<IEnumerable<Subscription>> GetSubscriptions();
        Task StoreNewRemoteVersion(StoreNewVersionCommand command);
        Task StoreNewRemoteOverallVersion(StoreNewOverallVersionCommand commandNewVersion);
    }

    public class StoreNewOverallVersionCommand
    {
        public DateTimeOffset NewVersion { get; set; }
        public Uri ServiceUri { get; set; }
    }

    public class StoreNewVersionCommand
    {
        public DateTimeOffset NewVersion { get; set; }
        public string EventType { get; set; }
    }
}