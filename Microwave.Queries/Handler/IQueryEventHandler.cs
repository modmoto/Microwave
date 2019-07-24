using System.Threading.Tasks;

namespace Microwave.Queries.Handler
{
    public interface IQueryEventHandler
    {
        Task Update();
    }
}