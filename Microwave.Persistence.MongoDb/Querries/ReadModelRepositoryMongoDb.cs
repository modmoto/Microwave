using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Queries;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class ReadModelRepositoryMongoDb : IReadModelRepository
    {
        private readonly IMongoDatabase _database;
        private string GetReadModelCollectionName<T>() => $"ReadModelDbos_{typeof(T).Name}";
        private string GetQuerryCollectionName<T>() => $"QueryDbos_{typeof(T).Name}";

        public ReadModelRepositoryMongoDb(MicrowaveMongoDb mongoDb)
        {
            _database = mongoDb.Database;
        }

        public async Task<Result<T>> LoadAsync<T>() where T : Query
        {
            var name = typeof(T).Name;
            var mongoCollection = _database.GetCollection<QueryDbo<T>>(GetQuerryCollectionName<T>());
            var query = (await mongoCollection.FindAsync(dbo => dbo.Type == typeof(T).Name)).FirstOrDefault();
            if (query == null) return Result<T>.NotFound(StringIdentity.Create(name));
            return Result<T>.Ok(query.Payload);
        }

        public async Task<Result<T>> LoadAsync<T>(Identity id) where T : IReadModel
        {
            if (id == null) return Result<T>.NotFound(null);
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var asyncCursor = await mongoCollection.FindAsync(dbo => dbo.Id == id.Id);
            var identifiableQueryDbo = asyncCursor.FirstOrDefault();
            if (identifiableQueryDbo == null) return Result<T>.NotFound(id);
            return Result<T>.Ok(identifiableQueryDbo.Payload);
        }

        public async Task<Result> SaveQueryAsync<T>(T query) where T : Query
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

        public async Task<Result> SaveReadModelAsync<T>(T readModel) where T : IReadModel, new()
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<ReadModelDbo<T>>();
            findOneAndReplaceOptions.IsUpsert = true;
            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<ReadModelDbo<T>, bool>>) (e => e.Id == readModel.Identity.Id),
                new ReadModelDbo<T>
                {
                    Id = readModel.Identity.Id,
                    Version = readModel.Version,
                    Payload = readModel
                }, findOneAndReplaceOptions);

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<T>>> LoadAllAsync<T>() where T : IReadModel
        {
            var mongoCollection = _database.GetCollection<ReadModelDbo<T>>(GetReadModelCollectionName<T>());
            var allElements = await mongoCollection.FindSync(_ => true).ToListAsync();
            if (!allElements.Any()) return Result<IEnumerable<T>>.Ok(new List<T>());
            return Result<IEnumerable<T>>.Ok(allElements.Select(r => r.Payload));
        }
    }
}