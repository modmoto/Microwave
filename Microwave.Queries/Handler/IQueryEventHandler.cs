using System.Threading.Tasks;

namespace Microwave.Queries.Handler
{
    internal interface IQueryEventHandler
    {
        Task Update();
    }
}