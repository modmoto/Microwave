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
        private readonly IObjectConverter _converter;

        public SnapShotRepository(IMongoDatabase context, IObjectConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Guid entityId) where T : new()
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo>("SnapShotDbos");
            var asyncCursor = await mongoCollection.FindAsync(r => r.EntityId == entityId.ToString());
            var snapShot = asyncCursor.ToList().FirstOrDefault();

            if (snapShot == null) return new DefaultSnapshot<T>();
            var data = _converter.Deserialize<T>(snapShot.Payload);
            return new SnapShotResult<T>(data, snapShot.Version);
        }

        public async Task SaveSnapShot<T>(T snapShot, Guid entityId, long version)
        {
            var mongoCollection = _context.GetCollection<SnapShotDbo>("SnapShotDbos");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<SnapShotDbo>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<SnapShotDbo, bool>>) (e => e.EntityId == entityId.ToString()),
                new SnapShotDbo
                {
                    EntityId = entityId.ToString(),
                    Version = version,
                    Payload = _converter.Serialize(snapShot)
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