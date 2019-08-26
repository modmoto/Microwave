using System;

namespace Microwave.Discovery.Subscriptions
{
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