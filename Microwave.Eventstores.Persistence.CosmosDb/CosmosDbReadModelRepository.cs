using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbReadModelRepository : IReadModelRepository
    {
        private readonly ICosmosDb _cosmosDb;
        private DocumentClient _client;
        private string GetReadModelCollectionName<T>() => $"ReadModelDbos_{typeof(T).Name}";
        private string GetQuerryCollectionName<T>() => $"QueryDbos_{typeof(T).Name}";

        public CosmosDbReadModelRepository(ICosmosDb cosmosDb)
        {
            _cosmosDb = cosmosDb;
            _client = _cosmosDb.GetCosmosDbClient();
        }

        public async Task<Result<T>> LoadAsync<T>() where T : Query
        {
            var name = typeof(T).Name;
            var result = Result<T>.NotFound(StringIdentity.Create(name));
            var queryResult = _client.CreateDocumentQuery<QueryDbo<T>>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, GetQuerryCollectionName<T>()),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.Type == name)
                .AsEnumerable().FirstOrDefault();
            if (queryResult != null)
            {
                result = Result<T>.Ok(queryResult.Payload);
            }

            return result;
        }

        public async Task<Result<T>> LoadAsync<T>(Identity id) where T : ReadModelBase
        {
            var name = typeof(T).Name;
            var queryResult = _client.CreateDocumentQuery<ReadModelDbo<T>>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, GetReadModelCollectionName<T>()),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(e => e.Id == id.Id)
                .AsEnumerable().FirstOrDefault();
            if (queryResult == null)
            {
                return Result<T>.NotFound(id);
            }

            return Result<T>.Ok(queryResult.Payload);
        }

        public async Task<Result> SaveQueryAsync<T>(T query) where T : Query
        {
            await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, GetQuerryCollectionName<T>()), query);
            return Result.Ok();
        }

        public async Task<Result> SaveReadModelAsync<T>(T readModel) where T : ReadModelBase, new()
        {
            try
            {
                await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, GetReadModelCollectionName<T>()), readModel);
            }
            catch (DocumentClientException)
            {
                var actualVersion = (await LoadAsync<T>(readModel.Identity)).Value.Version;
                return Result.ConcurrencyResult(readModel.Version, actualVersion);
            }
            return Result.Ok();
        }


        public async Task<Result<IEnumerable<T>>> LoadAllAsync<T>() where T : ReadModelBase
        {
            var name = typeof(T).Name;
            var queryResult = _client.CreateDocumentQuery<ReadModelDbo<T>>(
                    UriFactory.CreateDocumentCollectionUri(_cosmosDb.DatabaseId, GetReadModelCollectionName<T>()),
                    new FeedOptions { MaxItemCount = -1 })
                .AsEnumerable();
            return Result<IEnumerable<T>>.Ok(queryResult.Select(rm => rm.Payload).ToList());
        }
    }
}