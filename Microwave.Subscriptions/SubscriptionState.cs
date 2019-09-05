using System;

namespace Microwave.Subscriptions
{
    public class SubscriptionState
    {
        private readonly DateTimeOffset _oldVersion;

        public SubscriptionState(DateTimeOffset newVersion, DateTimeOffset oldVersion)
        {
            NewVersion = newVersion;
            _oldVersion = oldVersion;
        }

        public bool NeedsPush => NewVersion > _oldVersion;
        public DateTimeOffset NewVersion { get; }
    }
}