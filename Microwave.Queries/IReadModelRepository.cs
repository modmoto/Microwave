using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<ReadModelResult<T>> Load<T>(Identity id) where T : ReadModel;
        Task<Result> Save<T>(T query) where T : Query;
        Task<Result> Save<T>(T readModel, Identity identity, long version) where T : ReadModel, new();
        Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel;
    }
}