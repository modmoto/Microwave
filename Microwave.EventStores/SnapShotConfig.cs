using System.Collections.Generic;
using System.Linq;
using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores
{
    public class SnapShotConfig : ISnapShotConfig
    {
        private readonly IEnumerable<ISnapShotAfter> _snapShotAfters;

        public SnapShotConfig(IEnumerable<ISnapShotAfter> snapShotAfters = null)
        {
            _snapShotAfters = snapShotAfters ?? new List<ISnapShotAfter>();
        }
        public bool NeedSnapshot<T>(long snapShotVersion, long version) where T : IApply
        {
            var firstFoundSnapshot = _snapShotAfters.FirstOrDefault(s => s.EntityType == typeof(T));
            return firstFoundSnapshot?.DoesNeedSnapshot(snapShotVersion, version) ?? false;
        }
    }
}