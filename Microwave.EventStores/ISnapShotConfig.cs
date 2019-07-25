using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores
{
    public interface ISnapShotConfig
    {
        bool NeedSnapshot<T>(long snapShotVersion, long version) where T : IApply;
    }
}