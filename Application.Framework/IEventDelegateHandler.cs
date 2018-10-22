using System.Threading.Tasks;

namespace Application.Framework
{
    public interface IEventDelegateHandler
    {
        Task Update();
    }
}