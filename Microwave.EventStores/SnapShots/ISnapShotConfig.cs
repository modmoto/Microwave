using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores.SnapShots
{
    public interface ISnapShotConfig
    {
        bool NeedSnapshot<T>(long oldVersion, long newVersion) where T : IApply;
    }
}