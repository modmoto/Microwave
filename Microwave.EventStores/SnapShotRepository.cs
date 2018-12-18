using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Application;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class SnapShotRepository : ISnapShotRepository
    {
        private readonly IMongoDatabase _context;

        public SnapShotRepository(EventDatabase context)
        {
            _context = context.Database;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Guid entityId) where T : new()
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>("SnapShotDbos");
            var asyncCursor = await mongoCollection.FindAsync(r => r.EntityId == entityId.ToString());
            var snapShot = asyncCursor.ToList().FirstOrDefault();

            if (snapShot == null) return new DefaultSnapshot<T>();
            return new SnapShotResult<T>(snapShot.Payload, snapShot.Version);
        }

        public async Task SaveSnapShot<T>(T snapShot, Guid entityId, long version)
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>("SnapShotDbos");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<SnapShotDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<SnapShotDbo<T>, bool>>) (e => e.EntityId == entityId.ToString()),
                new SnapShotDbo<T>
                {
                    EntityId = entityId.ToString(),
                    Version = version,
                    Payload = snapShot
                }, findOneAndReplaceOptions);
        }
    }

    public class DefaultSnapshot<T> : SnapShotResult<T> where T : new()
    {
        public DefaultSnapshot() : base(new T(), 0)
        {
        }
    }
}