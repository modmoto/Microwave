using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<Type, object> _querryDictionary = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Identity, object>> _readModelDictionary = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Identity, object>> ();
        public Task<Result<T>> LoadAsync<T>() where T : Query
        {
            if (!_querryDictionary.TryGetValue(typeof(T), out var query)) return Task.FromResult(Result<T>.NotFound
            (StringIdentity.Create(nameof(T))));
            var queryParsed = query as T;
            return Task.FromResult(Result<T>.Ok(queryParsed));
        }

        public Task<Result<T>> LoadAsync<T>(Identity id) where T : ReadModelBase
        {
            if (!_readModelDictionary.TryGetValue(typeof(T), out var readModelDictionary))
                return Task.FromResult(Result<T>.NotFound(id));

            if (!readModelDictionary.TryGetValue(id, out var readModel))
                return Task.FromResult(Result<T>.NotFound(id));

            if (!(readModel is T model)) return Task.FromResult(Result<T>.NotFound(id));

            return Task.FromResult(Result<T>.Ok(model));
        }

        public Task<Result> SaveQueryAsync<T>(T query) where T : Query
        {
            _querryDictionary[typeof(T)] = query;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result> SaveReadModelAsync<T>(T readModel) where T : ReadModelBase, new()
        {
            if (!_readModelDictionary.TryGetValue(typeof(T), out var readModelDictionary))
                readModelDictionary = new ConcurrentDictionary<Identity, object>();
            readModelDictionary[readModel.Identity] = readModel;
            _readModelDictionary[typeof(T)] = readModelDictionary;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result<IEnumerable<T>>> LoadAllAsync<T>() where T : ReadModelBase
        {
            if (!_readModelDictionary.TryGetValue(typeof(T), out var readModelDictionary))
                return Task.FromResult(Result<IEnumerable<T>>.Ok(new List<T>()));

            var loadAll = readModelDictionary.Values.ToList();
            return Task.FromResult(Result<IEnumerable<T>>.Ok(loadAll.Select(l => (T) l)));
        }
    }
}