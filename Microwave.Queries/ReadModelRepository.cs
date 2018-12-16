using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.ObjectPersistences;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IObjectConverter _converter;

        public ReadModelRepository(IMongoDatabase database, ObjectConverter converter)
        {
            _database = database;
            _converter = converter;
        }

        public async Task<Result<T>> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var mongoCollection = _database.GetCollection<QueryDbo>("QueryDbos");
            var query = (await mongoCollection.FindAsync(dbo => dbo.Type == typeof(T).Name)).FirstOrDefault();
            if (query == null) return Result<T>.NotFound(name);
            var data = _converter.Deserialize<T>(query.Payload);
            return Result<T>.Ok(data);
        }

        public async Task<Result<ReadModelWrapper<T>>> Load<T>(Guid id) where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo>("IdentifiableQueryDbos");
            var asyncCursor = await mongoCollection.FindAsync(dbo => dbo.Id == id.ToString());
            var identifiableQueryDbo = asyncCursor.FirstOrDefault();
            if (identifiableQueryDbo == null || identifiableQueryDbo.QueryType != typeof(T).Name) return Result<ReadModelWrapper<T>>.NotFound(id.ToString());
            var deserialize = _converter.Deserialize<T>(identifiableQueryDbo.Payload);
            var wrapper = new ReadModelWrapper<T>(deserialize, id, identifiableQueryDbo.Version);
            return Result<ReadModelWrapper<T>>.Ok(wrapper);
        }

        public async Task<Result<IEnumerable<ReadModelWrapper<T>>>> LoadAll<T>() where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo>("IdentifiableQueryDbos");
            var querries = (await mongoCollection.FindAsync(q => q.QueryType == typeof(T).Name)).ToList();
            var readModelWrappers = querries.Select(q =>
                new ReadModelWrapper<T>(_converter.Deserialize<T>(q.Payload), new Guid(q.Id), q.Version));
            return Result<IEnumerable<ReadModelWrapper<T>>>.Ok(readModelWrappers);
        }

        public async Task<Result> Save(Query query)
        {
            var mongoCollection = _database.GetCollection<QueryDbo>("QueryDbos");
            await mongoCollection.InsertOneAsync(new QueryDbo
                {
                    Type = query.GetType().Name,
                    Payload = _converter.Serialize(query)
                });
            return Result.Ok();
        }

        public async Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new()
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo>("IdentifiableQueryDbos");
            await mongoCollection.InsertOneAsync(new IdentifiableQueryDbo
            {
                Id = readModelWrapper.Id.ToString(),
                Version = readModelWrapper.Version,
                QueryType = readModelWrapper.ReadModel.GetType().Name,
                Payload = _converter.Serialize(readModelWrapper.ReadModel)
            });
            return Result.Ok();
        }
    }
}