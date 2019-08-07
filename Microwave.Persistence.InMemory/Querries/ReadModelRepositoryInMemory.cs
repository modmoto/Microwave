using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Queries;

namespace Microwave.Persistence.InMemory.Querries
{
    public class ReadModelRepositoryInMemory : IReadModelRepository
    {
        private Dictionary<Type, object> querryDictionary = new Dictionary<Type, object>();
        private Dictionary<Identity, object> readModelDictionary = new Dictionary<Identity, object>();
        public Task<Result<T>> Load<T>() where T : Query
        {
            if (!querryDictionary.TryGetValue(typeof(T), out var query)) return Task.FromResult(Result<T>.NotFound
            (StringIdentity.Create(nameof(T))));
            var queryParsed = query as T;
            return Task.FromResult(Result<T>.Ok(queryParsed));
        }

        public Task<ReadModelResult<T>> Load<T>(Identity id) where T : ReadModel
        {
            if (id == null || !readModelDictionary.TryGetValue(id, out var readModel)) return Task.FromResult
            (ReadModelResult<T>.NotFound(id));
            var model = readModel as T;
            if (model == null) return Task.FromResult(ReadModelResult<T>.NotFound(id));

            return Task.FromResult(new ReadModelResult<T>(model, id, 0));
        }

        public Task<Result> Save<T>(T query) where T : Query
        {
            querryDictionary[typeof(T)] = query;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result> Save<T>(T readModel, Identity identity, long version) where T : ReadModel, new()
        {
            readModelDictionary[identity] = readModel;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel
        {
            var loadAll = readModelDictionary.Values.Where(v => v.GetType() == typeof(T)).ToList();
            if (loadAll.Count == 0) return Task.FromResult(Result<IEnumerable<T>>.NotFound(StringIdentity.Create(nameof(T))));
            return Task.FromResult(Result<IEnumerable<T>>.Ok(loadAll.Select(l => l as T)));
        }
    }
}