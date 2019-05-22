using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Domain.Results;
using Microwave.Persistence.MongoDb;
using MongoDB.Driver;

namespace Microwave.Queries.Persistence.MongoDb
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly IMongoDatabase _database;
        private string GetReadModelCollectionName<T>() => $"ReadModelDbos_{typeof(T).Name}";
        private string GetQuerryCollectionName<T>() => $"QueryDbos_{typeof(T).Name}";

        public ReadModelRepository(MicrowaveDatabase database)
        {
            _database = database.Database;
        }

        public async Task<Result<T>> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var mongoCollection = _database.GetCollection<QueryDbo<T>>(GetQuerryCollectionName<T>());
            var query = (await mongoCollection.FindAsync(dbo => dbo.Type == typeof(T).Name)).FirstOrDefault();
            if (query == null) return Result<T>.NotFound(StringIdentity.Create(name));
            return Result<T>.Ok(query.Payload);
        }

        public async Task<ReadModelResult<T>> Load<T>(Identity id) where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var asyncCursor = await mongoCollection.FindAsync(dbo => dbo.Id == id.Id);
            var identifiableQueryDbo = asyncCursor.FirstOrDefault();
            if (identifiableQueryDbo == null) return ReadModelResult<T>.NotFound(id);
            return ReadModelResult<T>.Ok(identifiableQueryDbo.Payload, id, identifiableQueryDbo.Version);
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

        public async Task<Result> Save<T>(ReadModelResult<T> readModelResult) where T : ReadModel, new()
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<ReadModelDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<ReadModelDbo<T>, bool>>) (e => e.Id == readModelResult.Id.Id),
                new ReadModelDbo<T>
                {
                    Id = readModelResult.Id.Id,
                    Version = readModelResult.Version,
                    Payload = readModelResult.Value
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var allElements = await mongoCollection.FindSync(_ => true).ToListAsync();
            if (!allElements.Any()) return Result<IEnumerable<T>>.NotFound(StringIdentity.Create(nameof(T)));
            return Result<IEnumerable<T>>.Ok(allElements.Select(r => r.Payload));
        }
    }
}