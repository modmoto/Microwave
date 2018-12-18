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

        public ReadModelRepository(ReadModelDatabase database)
        {
            _database = database.Database;
        }

        public async Task<Result<T>> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var mongoCollection = _database.GetCollection<QueryDbo<T>>("QueryDbos");
            var query = (await mongoCollection.FindAsync(dbo => dbo.Type == typeof(T).Name)).FirstOrDefault();
            if (query == null) return Result<T>.NotFound(name);
            return Result<T>.Ok(query.Payload);
        }

        public async Task<Result<ReadModelWrapper<T>>> Load<T>(Guid id) where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo<T>>("IdentifiableQueryDbos");
            var asyncCursor = await mongoCollection.FindAsync(dbo => dbo.Id == id.ToString());
            var identifiableQueryDbo = asyncCursor.FirstOrDefault();
            if (identifiableQueryDbo == null || identifiableQueryDbo.QueryType != typeof(T).Name) return Result<ReadModelWrapper<T>>.NotFound(id.ToString());
            var wrapper = new ReadModelWrapper<T>(identifiableQueryDbo.Payload, id, identifiableQueryDbo.Version);
            return Result<ReadModelWrapper<T>>.Ok(wrapper);
        }

        public async Task<Result<IEnumerable<ReadModelWrapper<T>>>> LoadAll<T>() where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo<T>>("IdentifiableQueryDbos");
            var querries = (await mongoCollection.FindAsync(q => q.QueryType == typeof(T).Name)).ToList();
            var readModelWrappers = querries.Select(q =>
                new ReadModelWrapper<T>(q.Payload, new Guid(q.Id), q.Version));
            return Result<IEnumerable<ReadModelWrapper<T>>>.Ok(readModelWrappers);
        }

        public async Task<Result> Save<T>(T query) where T : Query
         {
            var mongoCollection = _database.GetCollection<QueryDbo<T>>("QueryDbos");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<QueryDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            var querryName = query.GetType().Name;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<QueryDbo<T>, bool>>) (e => e.Type == querryName),
                new QueryDbo<T>
                {
                    Type = querryName,
                    Payload = query
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }

        public async Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new()
        {
            var mongoCollection = _database.GetCollection<IdentifiableQueryDbo<T>>("IdentifiableQueryDbos");

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<IdentifiableQueryDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<IdentifiableQueryDbo<T>, bool>>) (e => e.Id == readModelWrapper.Id.ToString()),
                new IdentifiableQueryDbo<T>
                {
                    Id = readModelWrapper.Id.ToString(),
                    Version = readModelWrapper.Version,
                    QueryType = readModelWrapper.ReadModel.GetType().Name,
                    Payload = readModelWrapper.ReadModel
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }
    }
}