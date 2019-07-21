using System;
using System.Threading.Tasks;

namespace Microwave.Queries
{
    public interface IAsyncEventHandler
    {
        Task Update();
        Type HandlerClassType { get; }
    }
}