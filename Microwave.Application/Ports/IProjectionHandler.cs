using System.Threading.Tasks;

namespace Microwave.Application.Ports
{
    public interface IProjectionHandler
    {
        Task Update();
    }
}