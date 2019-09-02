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

        protected bool Equals(Subscription other)
        {
            return SubscribedEvent == other.SubscribedEvent && Equals(SubscriberUrl, other.SubscriberUrl);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SubscribedEvent != null ? SubscribedEvent.GetHashCode() : 0) * 397) ^ (SubscriberUrl != null ? SubscriberUrl.GetHashCode() : 0);
            }
        }
    }
}