using System;

namespace Microwave.Domain.EventSourcing
{
    public class SnapShotAfter<T> : ISnapShotAfter where T : IApply
    {
        public int Writes { get; }
        public Type EntityType { get; }

        public SnapShotAfter(int writes)
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

    public interface ISnapShotAfter
    {
        bool DoesNeedSnapshot(long oldVersion, long currentVersion);
        Type EntityType { get; }
    }
}