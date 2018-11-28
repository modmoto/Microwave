using System.Threading.Tasks;

namespace Microwave.Application
{
    public interface IProjectionHandler
    {
        Task Update();
    }
}