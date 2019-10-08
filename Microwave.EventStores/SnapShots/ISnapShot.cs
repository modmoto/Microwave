using System;

namespace Microwave.EventStores.SnapShots
{
    public interface ISnapShot
    {
        bool DoesNeedSnapshot(long oldVersion, long currentVersion);
        Type EntityType { get; }
    }
}