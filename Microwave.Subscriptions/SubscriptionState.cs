using System;

namespace Microwave.Subscriptions
{
    public class SubscriptionState
    {
        private readonly DateTimeOffset _oldVersion;

        public SubscriptionState(DateTimeOffset newVersion, DateTimeOffset OldVersion)
        {
            NewVersion = newVersion;
            _oldVersion = OldVersion;
        }

        public bool NeedsPush => NewVersion > _oldVersion;
        public DateTimeOffset NewVersion { get; }
    }
}