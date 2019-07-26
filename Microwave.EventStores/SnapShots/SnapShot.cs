using System;
using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores.SnapShots
{
    public class SnapShot<T> : ISnapShot where T : IApply
    {
        public int Writes { get; }
        public Type EntityType { get; }

        public SnapShot(int writes)
        {
            Writes = writes;
            EntityType = typeof(T);
        }

        public bool DoesNeedSnapshot(long oldVersion, long currentVersion)
        {
            if (currentVersion % Writes == 0 && currentVersion != oldVersion) return true;
            if (currentVersion - oldVersion >= Writes) return true;
            if (currentVersion == oldVersion) return false;
            return (currentVersion - oldVersion + currentVersion) % Writes == 0;
        }
    }
}