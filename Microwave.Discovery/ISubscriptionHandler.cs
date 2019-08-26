using System;
using System.Threading.Tasks;

namespace Microwave.Discovery
{
    public interface ISubscriptionHandler
    {
        Task SubscribeOnDiscoveredServices();
        Task SubscribeForEvent(Subscription subscription);
        Task StoreSubscription(Subscription subscription);
        Task PushNewChanges();
    }

    public class Subscription
    {
        public Subscription(string subscribedEvent, Uri subscriberUrl)
        {
            SubscribedEvent = subscribedEvent;
            SubscriberUrl = subscriberUrl;
        }

        public string SubscribedEvent { get; }
        public Uri SubscriberUrl { get; }
    }
}