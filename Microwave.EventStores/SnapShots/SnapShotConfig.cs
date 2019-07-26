using System.Collections.Generic;
using System.Linq;
using Microwave.Domain.EventSourcing;

namespace Microwave.EventStores.SnapShots
{
    public class SnapShotConfig : ISnapShotConfig
    {
        private readonly IEnumerable<ISnapShot> _snapShotAfters;

        public SnapShotConfig(IEnumerable<ISnapShot> snapShotAfters = null)
        {
            _snapShotAfters = snapShotAfters ?? new List<ISnapShot>();
        }
        public bool NeedSnapshot<T>(long oldVersion, long newVersion) where T : IApply
        {
            var firstFoundSnapshot = _snapShotAfters.FirstOrDefault(s => s.EntityType == typeof(T));
            return firstFoundSnapshot?.DoesNeedSnapshot(oldVersion, newVersion) ?? false;
        }
    }
}