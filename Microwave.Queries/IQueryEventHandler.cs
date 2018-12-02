using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IQueryEventHandler
    {
        Task Update();
    }
}