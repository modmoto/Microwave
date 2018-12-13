using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Application;

namespace Microwave.EventStores
{
    public class SnapShotRepository : ISnapShotRepository
    {
        private readonly EventStoreContext _context;
        private readonly IObjectConverter _converter;

        public SnapShotRepository(EventStoreContext context, IObjectConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Guid entityId) where T : new()
        {
            var snapShot = await _context.SnapShots.FindAsync(entityId.ToString());
            if (snapShot == null) return new DefaultSnapshot<T>();
            var data = _converter.Deserialize<T>(snapShot.Payload);
            return new SnapShotResult<T>(data, snapShot.Version);
        }

        public async Task SaveSnapShot<T>(T snapShot, Guid entityId, long version)
        {
            await _context.SnapShots.Upsert(new SnapShotDbo
                {
                    EntityId = entityId.ToString(),
                    Payload = _converter.Serialize(snapShot),
                    Version = version
                })
                .RunAsync();
        }
    }

    public class DefaultSnapshot<T> : SnapShotResult<T> where T : new()
    {
        public DefaultSnapshot() : base(new T(), 0)
        {
        }
    }
}