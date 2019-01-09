using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class SnapShotRepository : ISnapShotRepository
    {
        private readonly IMongoDatabase _context;
        private readonly string _snapShotCollectionName = "SnapShotDbos";

        public SnapShotRepository(EventDatabase context)
        {
            _context = context.Database;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(string entityId) where T : new()
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>(_snapShotCollectionName);
            var asyncCursor = await mongoCollection.FindAsync(r => r.EntityId == entityId);
            var snapShot = asyncCursor.ToList().FirstOrDefault();

            if (snapShot == null) return new DefaultSnapshot<T>();
            return new SnapShotResult<T>(snapShot.Payload, snapShot.Version);
        }

        public async Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>(_snapShotCollectionName);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<SnapShotDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<SnapShotDbo<T>, bool>>) (e => e.EntityId == snapShot.Id),
                new SnapShotDbo<T>
                {
                    EntityId = snapShot.Id,
                    Version = snapShot.Version,
                    Payload = snapShot.Entity
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