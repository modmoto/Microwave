using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> LoadAsync<T>() where T : Query;
        Task<Result<T>> LoadAsync<T>(Identity id) where T : ReadModel;
        Task<Result> SaveQueryAsync<T>(T query) where T : Query;
        Task<Result> SaveReadModelAsync<T>(T readModel) where T : ReadModel, new();
        Task<Result<IEnumerable<T>>> LoadAllAsync<T>() where T : ReadModel;
    }
}