using System.Threading.Tasks;

namespace Application.Framework
{
    public interface IQeryRepository
    {
        Task<T> Load<T>() where T : Query;
        Task Save<T>(T query) where T : Query;
    }
}