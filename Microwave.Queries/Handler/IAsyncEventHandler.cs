using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Handler
{
    public interface IAsyncEventHandler
    {
        Task Update();
        Type HandlerClassType { get; }
    }
}