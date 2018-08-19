using System.Threading.Tasks;

namespace Adapters.Json.ObjectPersistences
{
    public interface IObjectPersister<T>
    {
        Task<T> GetAsync();
        Task Save(T querry);
    }
}