using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IObjectConverter _converter;

        public ReadModelRepository(ReadModelDatabase database, IObjectConverter converter)
        {
            _database = database.Database;
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

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<QueryDbo>();
            findOneAndReplaceOptions.IsUpsert = true;
            var querryName = query.GetType().Name;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<QueryDbo, bool>>) (e => e.Type == querryName),
                new QueryDbo
                {
                    Type = querryName,
                    Payload = _converter.Serialize(query)
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }

        public async Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new()
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo>("IdentifiableQueryDbos");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<IdentifiableQueryDbo>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<IdentifiableQueryDbo, bool>>) (e => e.Id == readModelWrapper.Id.ToString()),
                new IdentifiableQueryDbo
                {
                    Id = readModelWrapper.Id.ToString(),
                    Version = readModelWrapper.Version,
                    QueryType = readModelWrapper.ReadModel.GetType().Name,
                    Payload = _converter.Serialize(readModelWrapper.ReadModel)
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }
    }
}