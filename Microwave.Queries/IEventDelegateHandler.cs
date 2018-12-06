using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IEventDelegateHandler
    {
        Task Update();
    }
}