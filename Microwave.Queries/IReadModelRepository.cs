using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<Result<T>> Load<T>(Identity id) where T : ReadModel;
        Task<Result> Save<T>(T query) where T : Query;
        Task<Result> SaveReadModel<T>(T readModel) where T : ReadModel;
        Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel;
    }
}