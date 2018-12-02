using System.Threading.Tasks;

namespace Microwave.Application.Ports
{
    public interface IEventDelegateHandler
    {
        Task Update();
    }
}