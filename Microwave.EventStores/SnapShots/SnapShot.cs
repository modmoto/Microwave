using System;
using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores.SnapShots
{
    public class SnapShot<T> : ISnapShot where T : IApply
    {
        public int AmountDomainEvents { get; }
        public Type EntityType { get; }

        public SnapShot(int amountDomainEvents)
        {
            AmountDomainEvents = amountDomainEvents;
            EntityType = typeof(T);
        }

        public bool DoesNeedSnapshot(long oldVersion, long currentVersion)
        {
            if (currentVersion % AmountDomainEvents == 0 && currentVersion != oldVersion) return true;
            if (currentVersion - oldVersion >= AmountDomainEvents) return true;
            if (currentVersion == oldVersion) return false;
            return (currentVersion - oldVersion + currentVersion) % AmountDomainEvents == 0;
        }
    }
}