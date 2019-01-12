using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<Result<ReadModelWrapper<T>>> Load<T>(string id) where T : ReadModel;
        Task<Result<IEnumerable<ReadModelWrapper<T>>>> LoadAll<T>() where T : ReadModel;
        Task<Result> Save<T>(T query) where T : Query;
        Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new();
    }

    public class ReadModelWrapper<T> where T : ReadModel
    {
        public ReadModelWrapper(T readModel, string id, long version)
        {
            ReadModel = readModel;
            Id = id;
            Version = version;
        }

        public T ReadModel { get; }
        public string Id { get; }
        public long Version { get; }
    }
}