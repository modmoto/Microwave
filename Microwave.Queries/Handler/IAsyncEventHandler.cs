using System;
using System.Threading.Tasks;

namespace Microwave.Queries.Handler
{
    internal interface IAsyncEventHandler
    {
        Task Update();
        Type HandlerClassType { get; }
    }
}