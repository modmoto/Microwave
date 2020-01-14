using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.Results;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> LoadAsync<T>() where T : Query;
        Task<Result<T>> LoadAsync<T>(string id) where T : ReadModelBase;
        Task<Result<T>> LoadAsync<T>(Guid id) where T : ReadModelBase;
        Task<Result> SaveQueryAsync<T>(T query) where T : Query;
        Task<Result> SaveReadModelAsync<T>(T readModel) where T : ReadModelBase, new();
        Task<Result<IEnumerable<T>>> LoadAllAsync<T>() where T : ReadModelBase;
    }
}