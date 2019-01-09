using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly IMongoDatabase _database;
        private string GetReadModelCollectionName<T>() => $"ReadModelDbos_{typeof(T).Name}";
        private string GetQuerryCollectionName<T>() => $"QueryDbos_{typeof(T).Name}";

        public ReadModelRepository(ReadModelDatabase database)
        {
            _database = database.Database;
        }

        public async Task<Result<T>> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var mongoCollection = _database.GetCollection<QueryDbo<T>>(GetQuerryCollectionName<T>());
            var query = (await mongoCollection.FindAsync(dbo => dbo.Type == typeof(T).Name)).FirstOrDefault();
            if (query == null) return Result<T>.NotFound(name);
            return Result<T>.Ok(query.Payload);
        }

        public async Task<Result<ReadModelWrapper<T>>> Load<T>(string id) where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var asyncCursor = await mongoCollection.FindAsync(dbo => dbo.Id == id);
            var identifiableQueryDbo = asyncCursor.FirstOrDefault();
            if (identifiableQueryDbo == null) return Result<ReadModelWrapper<T>>.NotFound(id);
            var wrapper = new ReadModelWrapper<T>(identifiableQueryDbo.Payload, id, identifiableQueryDbo.Version);
            return Result<ReadModelWrapper<T>>.Ok(wrapper);
        }

        public async Task<Result<IEnumerable<ReadModelWrapper<T>>>> LoadAll<T>() where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var querries = await mongoCollection.Find(_ => true).ToListAsync();
            var readModelWrappers = querries.Select(q => new ReadModelWrapper<T>(q.Payload, q.Id, q.Version));
            return Result<IEnumerable<ReadModelWrapper<T>>>.Ok(readModelWrappers);
        }

        public async Task<Result> Save<T>(T query) where T : Query
         {
            var mongoCollection = _database.GetCollection<QueryDbo<T>>(GetQuerryCollectionName<T>());

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
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<ReadModelDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<ReadModelDbo<T>, bool>>) (e => e.Id == readModelWrapper.Id),
                new ReadModelDbo<T>
                {
                    Id = readModelWrapper.Id,
                    Version = readModelWrapper.Version,
                    Payload = readModelWrapper.ReadModel
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }
    }
}