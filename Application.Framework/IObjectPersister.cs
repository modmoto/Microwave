using System.Threading.Tasks;

namespace Application.Framework
{
    public interface IObjectPersister<T>
    {
        Task<T> GetAsync();
        Task Save(T querry);
    }
}