using System.Threading.Tasks;

namespace Microwave.Queries.Handler
{
    public interface IEventHandler
    {
        Task UpdateAsync();
    }
}