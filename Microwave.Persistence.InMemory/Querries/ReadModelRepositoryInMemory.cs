using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Queries;

namespace Microwave.Persistence.InMemory.Querries
{
    public class ReadModelRepositoryInMemory : IReadModelRepository
    {
        public Task<Result<T>> Load<T>() where T : Query
        {
            throw new NotImplementedException();
        }

        public Task<ReadModelResult<T>> Load<T>(Identity id) where T : ReadModel
        {
            throw new NotImplementedException();
        }

        public Task<Result> Save<T>(T query) where T : Query
        {
            throw new NotImplementedException();
        }

        public Task<Result> Save<T>(T readModel, Identity identity, long version) where T : ReadModel, new()
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel
        {
            throw new NotImplementedException();
        }
    }
}