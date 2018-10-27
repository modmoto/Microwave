using System.Threading.Tasks;

namespace Application.Framework
{
    public interface IProjectionHandler
    {
        Task Update();
    }
}