using System.Threading.Tasks;

namespace Adapters.Json.ObjectPersistences
{
    public interface IObjectPersister
    {
        Task<T> GetAsync<T>();
        Task Save<T>(T querry);
    }
}