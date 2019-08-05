using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.EventStores.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    public class SnapShotRepository : ISnapShotRepository
    {
        private readonly IMongoDatabase _context;
        private readonly string _snapShotCollectionName = "SnapShotDbos";

        public SnapShotRepository(MicrowaveMongoDb context)
        {
            _context = context.Database;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new()
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>(_snapShotCollectionName);
            var asyncCursor = await mongoCollection.FindAsync(r => r.Id == entityId.Id);
            var snapShot = asyncCursor.ToList().FirstOrDefault();

            if (snapShot == null) return SnapShotResult<T>.Default();
            return new SnapShotResult<T>(snapShot.Payload, snapShot.Version);
        }

        public async Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo<T>>(_snapShotCollectionName);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<SnapShotDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<SnapShotDbo<T>, bool>>) (e => e.Id == snapShot.Id.Id),
                new SnapShotDbo<T>
                {
                    Id = snapShot.Id.Id,
                    Version = snapShot.Version,
                    Payload = snapShot.Entity
                }, findOneAndReplaceOptions);
        }
    }
}