using System;

namespace Microwave.EventStores
{
    public class SnapShotAfter : Attribute
    {
        public int Writes { get; }

        public SnapShotAfter(int writes)
        {
            Writes = writes;
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