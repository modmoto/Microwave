using System;
using System.Threading.Tasks;
using Application.Framework.Results;

namespace Application.Framework
{
    public interface IQeryRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<Result<T>> Load<T>(Guid id) where T : IdentifiableQuery;
        Task<Result> Save(Query query);
        Task<Result> Save(IdentifiableQuery query);
    }
}