using System;
using System.Threading.Tasks;
using Microwave.Application;

namespace Microwave.EventStores
{
    public class SnapShotRepository : ISnapShotRepository
    {
        private readonly EventStoreContext _context;
        private readonly IObjectConverter _converter;
        private readonly object _lock = new object();

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

        public Task SaveSnapShot<T>(T snapShot, Guid entityId, long version)
        {
            lock (_lock)
            {
                var firstOrDefault = _context.SnapShots.Find(entityId.ToString());
                if (firstOrDefault != null)
                {
                    firstOrDefault.Payload = _converter.Serialize(snapShot);
                    firstOrDefault.EntityId = entityId.ToString();
                    firstOrDefault.Version = 0;
                    _context.Update(firstOrDefault);
                }
                else
                {
                    var snapShotDbo = new SnapShotDbo
                    {
                        EntityId = entityId.ToString(),
                        Payload = _converter.Serialize(snapShot),
                        Version = version
                    };
                    _context.SnapShots.Add(snapShotDbo);
                }

                _context.SaveChanges();
            }

            return Task.CompletedTask;
        }
    }

    public class Identifiable
    {
        public Guid Id { get; }
    }


    public class DefaultSnapshot<T> : SnapShotResult<T> where T : new()
    {
        public DefaultSnapshot() : base(new T(), 0)
        {
        }
    }
}