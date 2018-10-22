using System;
using System.Threading.Tasks;

namespace Application.Framework
{
    public interface IQeryRepository
    {
        Task<T> Load<T>() where T : Query;
        Task<T> Load<T>(Guid id) where T : IdentifiableQuery;
        Task Save(Query query);
        Task Save(IdentifiableQuery query);
    }
}