using System;
using System.Threading.Tasks;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public interface IQeryRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<Result<ReadModelWrapper<T>>> Load<T>(Guid id) where T : IdentifiableQuery;
        Task<Result> Save(Query query);
        Task<Result> SaveById<TQuerry>(ReadModelWrapper<TQuerry> readModelWrapper) where TQuerry : IdentifiableQuery, new();
    }

    public class ReadModelWrapper<T> where T : IdentifiableQuery
    {
        public ReadModelWrapper(T readModel, Guid id, long version)
        {
            ReadModel = readModel;
            Id = id;
            Version = version;
        }

        public T ReadModel { get; }
        public Guid Id { get; }
        public long Version { get; }
    }
}